using System.Diagnostics;
using System.IO.Compression;

namespace ZipAll.Core;

public static class ArchiveWriter
{
    private const long InMemoryCompressionTestThreshold = 64L * 1024 * 1024;

    public static async Task<ArchiveResult> CreateArchiveAsync(
        string sourceDirectory,
        string destinationZipPath,
        ExclusionEngine? exclusions = null,
        ZipCompressionMode compressionMode = ZipCompressionMode.Deflate,
        IProgress<ArchiveFileEntry>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

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

        await using (var zipStream = new FileStream(
            destinationZipPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ZipArchiveEntry? zipEntry = null;

                try
                {
                    var entryName = entry.RelativePath.Replace(Path.DirectorySeparatorChar, '/');
                    var rawLength = new FileInfo(entry.FullPath).Length;

                    byte[]? bufferedContent = null;
                    CompressionLevel effectiveLevel;

                    if (compressionMode == ZipCompressionMode.Stored)
                    {
                        effectiveLevel = CompressionLevel.NoCompression;
                    }
                    else if (rawLength > 0 && rawLength <= InMemoryCompressionTestThreshold)
                    {
                        bufferedContent = await File.ReadAllBytesAsync(entry.FullPath, cancellationToken);
                        effectiveLevel = await IsDeflateBeneficialAsync(bufferedContent, cancellationToken)
                            ? CompressionLevel.Optimal
                            : CompressionLevel.NoCompression;
                    }
                    else
                    {
                        effectiveLevel = CompressionLevel.Optimal;
                    }

                    zipEntry = archive.CreateEntry(entryName, effectiveLevel);
                    zipEntry.ExternalAttributes = ToDosAttributeBits(entry.Attributes);

                    await using (var entryStream = zipEntry.Open())
                    {
                        if (bufferedContent is not null)
                        {
                            await entryStream.WriteAsync(bufferedContent, cancellationToken);
                        }
                        else
                        {
                            await using var sourceStream = new FileStream(
                                entry.FullPath,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read,
                                bufferSize: 81920,
                                useAsync: true);

                            await sourceStream.CopyToAsync(entryStream, cancellationToken);
                        }
                    }

                    totalRawBytes += zipEntry.Length;
                    totalCompressedBytes += zipEntry.CompressedLength;
                    writtenCount++;

                    if (effectiveLevel == CompressionLevel.NoCompression)
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
                    zipEntry?.Delete();
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

    private static int ToDosAttributeBits(FileAttributes attributes)
    {
        var dosAttributes = 0;

        if (attributes.HasFlag(FileAttributes.ReadOnly))
        {
            dosAttributes |= 0x01;
        }

        if (attributes.HasFlag(FileAttributes.Hidden))
        {
            dosAttributes |= 0x02;
        }

        if (attributes.HasFlag(FileAttributes.System))
        {
            dosAttributes |= 0x04;
        }

        if (attributes.HasFlag(FileAttributes.Archive))
        {
            dosAttributes |= 0x20;
        }

        return dosAttributes;
    }

    private static async Task<bool> IsDeflateBeneficialAsync(byte[] rawContent, CancellationToken cancellationToken)
    {
        using var compressedStream = new MemoryStream();

        await using (var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            await deflateStream.WriteAsync(rawContent, cancellationToken);
        }

        return compressedStream.Length < rawContent.LongLength;
    }
}
