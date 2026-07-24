using ZipAll.Core;

namespace ZipAll.Core.Tests;

public class EncryptedArchiveWriterTests
{
    [Fact]
    public async Task CreateArchiveAsync_EmptyPassword_Throws()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        await Assert.ThrowsAsync<ArgumentException>(
            () => EncryptedArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, string.Empty));
    }

    [Fact]
    public async Task CreateArchiveAsync_CorrectPassword_VerifiesAndReadsBackContent()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", "alpha");
        temp.CreateFile("nested/b.txt", "beta");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await EncryptedArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, "correct-horse-battery-staple");

        Assert.Equal(2, result.EntryCount);

        var verification = EncryptedArchiveVerifier.Verify(zipPath, "correct-horse-battery-staple", result.EntryCount);
        Assert.True(verification.Success);
    }

    [Fact]
    public async Task CreateArchiveAsync_WrongPassword_FailsVerification()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", "alpha");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await EncryptedArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, "correct-password");

        var verification = EncryptedArchiveVerifier.Verify(zipPath, "wrong-password", result.EntryCount);
        Assert.False(verification.Success);
    }

    [Fact]
    public async Task CreateArchiveAsync_WithExclusions_OmitsExcludedEntries()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("keep.txt", "keep");
        temp.CreateFile("skip.tmp", "skip");
        var exclusions = new ExclusionEngine(filePatterns: new[] { "*.tmp" });
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await EncryptedArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, "pw", exclusions);

        Assert.Equal(1, result.EntryCount);
    }

    [Fact]
    public async Task CreateArchiveAsync_StoredMode_StillEncryptsAndRoundTrips()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", "some content");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        var result = await EncryptedArchiveWriter.CreateArchiveAsync(
            temp.Path, zipPath, "pw", compressionMode: ZipCompressionMode.Stored);

        Assert.Equal(1, result.StoredEntryCount);
        var verification = EncryptedArchiveVerifier.Verify(zipPath, "pw", result.EntryCount);
        Assert.True(verification.Success);
    }

    [Fact]
    public async Task CreateArchiveAsync_UnprotectedReader_CannotDecryptContent()
    {
        using var temp = new TempDirectory();
        using var outDir = new TempDirectory();
        temp.CreateFile("a.txt", "secret content");
        var zipPath = System.IO.Path.Combine(outDir.Path, "archive.zip");

        await EncryptedArchiveWriter.CreateArchiveAsync(temp.Path, zipPath, "pw");

        using var archive = System.IO.Compression.ZipFile.OpenRead(zipPath);
        var entry = Assert.Single(archive.Entries);

        Assert.ThrowsAny<Exception>(() =>
        {
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            _ = reader.ReadToEnd();
        });
    }
}
