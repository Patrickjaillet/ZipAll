using ZipAll.Core;

var workDir = Path.Combine(Path.GetTempPath(), "ZipAllManualHarness_" + Guid.NewGuid().ToString("N"));
var sourceDir = Path.Combine(workDir, "source");
var zipPath = Path.Combine(workDir, "output.zip");

Directory.CreateDirectory(sourceDir);
Directory.CreateDirectory(Path.Combine(sourceDir, "nested", "deeper"));
Directory.CreateDirectory(Path.Combine(sourceDir, "empty-folder"));

File.WriteAllText(Path.Combine(sourceDir, "root-file.txt"), "Hello from the root.");
File.WriteAllText(Path.Combine(sourceDir, "nested", "nested-file.txt"), "Hello from a nested folder.");
File.WriteAllText(Path.Combine(sourceDir, "nested", "deeper", "deep-file.txt"), "Hello from a deeply nested folder.");
File.WriteAllText(Path.Combine(sourceDir, "unicode-\u00e9\u00e0\u4e2d\u6587-file.txt"), "Unicode file name round trip.");
File.WriteAllBytes(Path.Combine(sourceDir, "empty-file.bin"), Array.Empty<byte>());

var readOnlyFilePath = Path.Combine(sourceDir, "readonly-file.txt");
File.WriteAllText(readOnlyFilePath, "Read-only content.");
File.SetAttributes(readOnlyFilePath, File.GetAttributes(readOnlyFilePath) | FileAttributes.ReadOnly);

var hiddenFilePath = Path.Combine(sourceDir, "hidden-file.txt");
File.WriteAllText(hiddenFilePath, "Hidden content.");
File.SetAttributes(hiddenFilePath, File.GetAttributes(hiddenFilePath) | FileAttributes.Hidden);

var systemFilePath = Path.Combine(sourceDir, "system-file.txt");
File.WriteAllText(systemFilePath, "System content.");
File.SetAttributes(systemFilePath, File.GetAttributes(systemFilePath) | FileAttributes.System);

var longNestedDirectory = sourceDir;
while (longNestedDirectory.Length < 300)
{
    longNestedDirectory = Path.Combine(longNestedDirectory, "long-path-segment");
}

Directory.CreateDirectory(longNestedDirectory);
var longPathFilePath = Path.Combine(longNestedDirectory, "long-path-file.txt");
File.WriteAllText(longPathFilePath, "Content behind a path longer than MAX_PATH.");

Console.WriteLine($"Long test path length : {longPathFilePath.Length} characters");

var lockedFilePath = Path.Combine(sourceDir, "locked-file.txt");
File.WriteAllText(lockedFilePath, "This file will be held open exclusively while the archive is built.");

var expectedEntryCount = DirectoryWalker.EnumerateFiles(sourceDir).Count();

Console.WriteLine($"Source directory : {sourceDir}");
Console.WriteLine($"Archive path     : {zipPath}");
Console.WriteLine($"Expected entries : {expectedEntryCount}");
Console.WriteLine();

var progress = new Progress<ArchiveFileEntry>(entry => Console.WriteLine($"  added: {entry.RelativePath}"));

ArchiveResult result;
using (new FileStream(lockedFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
{
    result = await ArchiveWriter.CreateArchiveAsync(sourceDir, zipPath, exclusions: null, progress: progress);
}

Console.WriteLine();
Console.WriteLine($"Archive created  : {result.EntryCount} entries ({result.DeflatedEntryCount} deflated, {result.StoredEntryCount} stored)");
Console.WriteLine($"Original size    : {result.TotalBytesWritten} bytes");
Console.WriteLine($"Compressed size  : {result.TotalCompressedBytes} bytes ({result.CompressionRatio:P1} smaller)");
Console.WriteLine($"Elapsed          : {result.Elapsed.TotalMilliseconds:F0} ms");

var lockedFileHandledGracefully =
    result.SkippedEntries.Count == 1 &&
    result.SkippedEntries[0].Path == "locked-file.txt" &&
    result.EntryCount == expectedEntryCount - 1;

Console.WriteLine(lockedFileHandledGracefully
    ? "LOCKED-FILE CHECK PASSED: the locked file was skipped and reported instead of aborting the archive."
    : $"LOCKED-FILE CHECK FAILED: expected exactly 1 skipped entry for 'locked-file.txt', got {result.SkippedEntries.Count}.");

var verification = ArchiveVerifier.Verify(zipPath, result.EntryCount);

Console.WriteLine();
if (verification.Success)
{
    Console.WriteLine($"VERIFICATION PASSED: {verification.ActualEntryCount}/{verification.ExpectedEntryCount} entries read back and CRC32-checked cleanly.");
}
else
{
    Console.WriteLine($"VERIFICATION FAILED: {verification.FailureReason}");
}

var attributesPreserved = AreDosAttributesPreserved(zipPath);
Console.WriteLine(attributesPreserved
    ? "ATTRIBUTE CHECK PASSED: read-only/hidden/system bits round-tripped into the archive entries."
    : "ATTRIBUTE CHECK FAILED: read-only/hidden/system bits were not found on the expected archive entries.");

foreach (var attributedFile in new[] { readOnlyFilePath, hiddenFilePath, systemFilePath })
{
    File.SetAttributes(attributedFile, FileAttributes.Normal);
}

Directory.Delete(workDir, recursive: true);

return verification.Success && attributesPreserved && lockedFileHandledGracefully ? 0 : 1;

static bool AreDosAttributesPreserved(string zipPath)
{
    using var archive = System.IO.Compression.ZipFile.OpenRead(zipPath);

    bool HasBit(string entryName, int bit)
    {
        var entry = archive.Entries.FirstOrDefault(e => e.Name == entryName);
        return entry is not null && (entry.ExternalAttributes & bit) == bit;
    }

    return HasBit("readonly-file.txt", 0x01)
        && HasBit("hidden-file.txt", 0x02)
        && HasBit("system-file.txt", 0x04);
}
