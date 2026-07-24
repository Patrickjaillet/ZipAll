using ZipAll.Core;

namespace ZipAll.Core.Tests;

public class DirectoryWalkerTests
{
    [Fact]
    public void EnumerateFiles_ThrowsDirectoryNotFoundException_WhenRootDoesNotExist()
    {
        var missingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ZipAllTests_Missing_" + Guid.NewGuid().ToString("N"));

        Assert.Throws<DirectoryNotFoundException>(() => DirectoryWalker.EnumerateFiles(missingPath).ToList());
    }

    [Fact]
    public void EnumerateFiles_EmptyDirectory_ReturnsNoEntries()
    {
        using var temp = new TempDirectory();

        var entries = DirectoryWalker.EnumerateFiles(temp.Path).ToList();

        Assert.Empty(entries);
    }

    [Fact]
    public void EnumerateFiles_EmptyNestedSubdirectories_AreTraversedButYieldNoEntries()
    {
        using var temp = new TempDirectory();
        temp.CreateDirectory("a/b/c");
        temp.CreateDirectory("a/b/d");

        var entries = DirectoryWalker.EnumerateFiles(temp.Path).ToList();

        Assert.Empty(entries);
    }

    [Fact]
    public void EnumerateFiles_DeeplyNestedTree_ReturnsAllFilesWithCorrectRelativePaths()
    {
        using var temp = new TempDirectory();
        var depth = 15;
        var relativeSegments = Enumerable.Range(0, depth).Select(i => $"L{i}");
        var deepRelativeDir = string.Join("/", relativeSegments);
        temp.CreateFile($"{deepRelativeDir}/leaf.txt", "deep");
        temp.CreateFile("top.txt", "shallow");

        var entries = DirectoryWalker.EnumerateFiles(temp.Path).ToList();

        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, e => e.RelativePath == "top.txt");
        var deepEntry = Assert.Single(entries, e => e.RelativePath.EndsWith("leaf.txt"));
        Assert.Equal(depth, deepEntry.RelativePath.Split(System.IO.Path.DirectorySeparatorChar).Length - 1);
    }

    [Fact]
    public void EnumerateFiles_SpecialCharactersInNames_AreHandledCorrectly()
    {
        using var temp = new TempDirectory();
        var names = new[]
        {
            "space name.txt",
            "unicode-éèà-中文.txt",
            "brackets[1](2).txt",
            "hash#and&ampersand.txt",
            "trailing.dot..txt"
        };

        foreach (var name in names)
        {
            temp.CreateFile(name);
        }

        var entries = DirectoryWalker.EnumerateFiles(temp.Path).ToList();

        Assert.Equal(names.Length, entries.Count);
        foreach (var name in names)
        {
            Assert.Contains(entries, e => e.RelativePath == name);
        }
    }

    [Fact]
    public void EnumerateFiles_LargeFlatTree_ReturnsEveryFile()
    {
        using var temp = new TempDirectory();
        const int fileCount = 500;

        for (var i = 0; i < fileCount; i++)
        {
            temp.CreateFile($"file_{i:D4}.txt", "x");
        }

        var entries = DirectoryWalker.EnumerateFiles(temp.Path).ToList();

        Assert.Equal(fileCount, entries.Count);
    }

    [Fact]
    public void EnumerateFiles_WithExclusions_SkipsMatchingFilesAndDirectories()
    {
        using var temp = new TempDirectory();
        temp.CreateFile("keep.txt");
        temp.CreateFile("skip.tmp");
        temp.CreateFile("excluded_dir/inside.txt");
        var exclusions = new ExclusionEngine(
            directoryPatterns: new[] { "excluded_dir" },
            filePatterns: new[] { "*.tmp" });

        var entries = DirectoryWalker.EnumerateFiles(temp.Path, exclusions).ToList();

        Assert.Single(entries);
        Assert.Equal("keep.txt", entries[0].RelativePath);
    }
}
