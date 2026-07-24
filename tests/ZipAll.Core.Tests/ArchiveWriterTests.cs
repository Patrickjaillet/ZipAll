using System.IO.Compression;
using ZipAll.Core;

namespace ZipAll.Core.Tests;

public class ArchiveWriterTests
{
    [Fact]
    public async Task CreateArchiveAsync_EmptySourceDirectory_ProducesValidEmptyArchive()
    {
        using var temp = new TempDirectory();
        var zipPath = System.IO.Path.Combine(temp.Path, "..", "empty.zip");

        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath);

        Assert.Equal(0, result.EntryCount);
        Assert.Empty(result.SkippedEntries);

        var verification = ArchiveVerifier.Verify(zipPath, expectedEntryCount: 0);
        Assert.True(verification.Success);

        File.Delete(zipPath);
    }

    [Fact]
    public async Task CreateArchiveAsync_FlatFiles_RoundTripsContentCorrectly()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", "alpha");
        temp.CreateFile("b.txt", "beta");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath);

        Assert.Equal(2, result.EntryCount);

        using var archive = ZipFile.OpenRead(zipPath);
        Assert.Equal(2, archive.Entries.Count);

        var entryA = archive.GetEntry("a.txt");
        Assert.NotNull(entryA);
        using (var reader = new StreamReader(entryA!.Open()))
        {
            Assert.Equal("alpha", await reader.ReadToEndAsync());
        }
    }

    [Fact]
    public async Task CreateArchiveAsync_DeeplyNestedTree_PreservesRelativeStructureAsForwardSlashes()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a/b/c/d/e/leaf.txt", "deep content");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath);

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = Assert.Single(archive.Entries);
        Assert.Equal("a/b/c/d/e/leaf.txt", entry.FullName);
    }

    [Fact]
    public async Task CreateArchiveAsync_SpecialCharacterFileNames_ArchivedAndReadableByName()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        var name = "unicode-éèà-中文 & (brackets).txt";
        temp.CreateFile(name, "special");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath);

        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry(name);
        Assert.NotNull(entry);
    }

    [Fact]
    public async Task CreateArchiveAsync_LargeFileCount_ArchivesEveryFile()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        const int fileCount = 300;

        for (var i = 0; i < fileCount; i++)
        {
            temp.CreateFile($"file_{i:D4}.txt", $"content-{i}");
        }

        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");
        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath);

        Assert.Equal(fileCount, result.EntryCount);

        var verification = ArchiveVerifier.Verify(zipPath, expectedEntryCount: fileCount);
        Assert.True(verification.Success);
    }

    [Fact]
    public async Task CreateArchiveAsync_WithExclusions_OmitsExcludedEntriesFromArchive()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("keep.txt", "keep");
        temp.CreateFile("skip.tmp", "skip");
        var exclusions = new ExclusionEngine(filePatterns: new[] { "*.tmp" });
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, exclusions);

        Assert.Equal(1, result.EntryCount);
        using var archive = ZipFile.OpenRead(zipPath);
        Assert.NotNull(archive.GetEntry("keep.txt"));
        Assert.Null(archive.GetEntry("skip.tmp"));
    }

    [Fact]
    public async Task CreateArchiveAsync_StoredMode_UsesNoCompressionForEveryEntry()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", new string('a', 10_000));
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, compressionMode: ZipCompressionMode.Stored);

        Assert.Equal(1, result.EntryCount);
        Assert.Equal(1, result.StoredEntryCount);
        Assert.Equal(0, result.DeflatedEntryCount);
    }

    [Fact]
    public async Task CreateArchiveAsync_DeflateMode_CompressesHighlyCompressibleContent()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", new string('a', 100_000));
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, compressionMode: ZipCompressionMode.Deflate);

        Assert.Equal(1, result.DeflatedEntryCount);
        Assert.True(result.TotalCompressedBytes < result.TotalBytesWritten);
    }

    [Fact]
    public async Task CreateArchiveAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        for (var i = 0; i < 20; i++)
        {
            temp.CreateFile($"file_{i}.txt", new string('x', 1000));
        }

        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, cancellationToken: cts.Token));
    }
}
