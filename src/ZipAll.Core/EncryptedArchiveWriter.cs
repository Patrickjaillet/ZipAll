using ICSharpCode.SharpZipLib.Zip;

namespace ZipAll.Core;

public static class EncryptedArchiveWriter
{
    public static async Task<ArchiveResult> CreateArchiveAsync(
        string sourceDirectory,
        string destinationZipPath,
        string password,
        ExclusionEngine? exclusions = null,
        ZipCompressionMode compressionMode = ZipCompressionMode.Deflate,
        IProgress<ArchiveFileEntry>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("A non-empty password is required to create an encrypted archive.", nameof(password));
        }

        return await Task.Run(
            () => CreateArchive(sourceDirectory, destinationZipPath, password, exclusions, compressionMode, progress, cancellationToken),
            cancellationToken);
    }

    private static ArchiveResult CreateArchive(
        string sourceDirectory,
        string destinationZipPath,
        string password,
        ExclusionEngine? exclusions,
        ZipCompressionMode compressionMode,
        IProgress<ArchiveFileEntry>? progress,
        CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var root = Path.GetFullPath(sourceDirectory);
        var skippedEntries = new List<SkippedEntry>();
        var entries = DirectoryWalker.EnumerateFiles(root, exclusions, skippedEntries).ToList();

        var destinationDirectory = Path.GetDirectoryName(Path.GetFullPath(destinationZipPath));
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        long totalRawBytes = 0;
        long totalCompressedBytes = 0;
        var storedCount = 0;
        var deflatedCount = 0;
        var writtenCount = 0;

        var compressionLevel = compressionMode == ZipCompressionMode.Stored ? 0 : 9;

        using (var zipStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var archive = new ZipOutputStream(zipStream))
        {
            archive.Password = password;
            archive.SetLevel(compressionLevel);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var entryName = entry.RelativePath.Replace(Path.DirectorySeparatorChar, '/');
                    var rawLength = new FileInfo(entry.FullPath).Length;
                    var positionBeforeEntry = zipStream.Position;

                    var zipEntry = new ZipEntry(entryName)
                    {
                        DateTime = File.GetLastWriteTime(entry.FullPath),
                        AESKeySize = 256,
                        Size = rawLength,
                    };

                    archive.PutNextEntry(zipEntry);

                    using (var sourceStream = new FileStream(entry.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        sourceStream.CopyTo(archive);
                    }

                    archive.CloseEntry();

                    totalRawBytes += rawLength;
                    totalCompressedBytes += zipStream.Position - positionBeforeEntry;
                    writtenCount++;

                    if (compressionMode == ZipCompressionMode.Stored)
                    {
                        storedCount++;
                    }
                    else
                    {
                        deflatedCount++;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    skippedEntries.Add(new SkippedEntry(entry.RelativePath, ex.Message));
                }
                finally
                {
                    progress?.Report(entry);
                }
            }
        }

        stopwatch.Stop();

        return new ArchiveResult(
            writtenCount,
            totalRawBytes,
            totalCompressedBytes,
            storedCount,
            deflatedCount,
            stopwatch.Elapsed,
            skippedEntries);
    }
}
