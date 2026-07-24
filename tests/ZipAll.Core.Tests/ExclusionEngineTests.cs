using ZipAll.Core;

namespace ZipAll.Core.Tests;

public class ExclusionEngineTests
{
    [Fact]
    public void IsFileExcluded_ReturnsFalse_WhenNoPatternsConfigured()
    {
        var engine = new ExclusionEngine();

        Assert.False(engine.IsFileExcluded(@"C:\root\file.txt", "file.txt"));
    }

    [Theory]
    [InlineData("*.log", "logs/app.log", true)]
    [InlineData("*.log", "logs/app.txt", false)]
    [InlineData("secret.txt", "nested/deep/secret.txt", true)]
    [InlineData("secret.txt", "nested/deep/other.txt", false)]
    public void IsFileExcluded_MatchesByFileNameOrRelativePath(string pattern, string relativePath, bool expected)
    {
        var engine = new ExclusionEngine(filePatterns: new[] { pattern });
        var fullPath = Path.Combine(@"C:\root", relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.Equal(expected, engine.IsFileExcluded(fullPath, relativePath));
    }

    [Theory]
    [InlineData("bin", "bin", true)]
    [InlineData("bin", "src/bin", true)]
    [InlineData("bin", "binary", false)]
    public void IsDirectoryExcluded_MatchesByDirectoryNameOrRelativePath(string pattern, string relativePath, bool expected)
    {
        var engine = new ExclusionEngine(directoryPatterns: new[] { pattern });
        var fullPath = Path.Combine(@"C:\root", relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.Equal(expected, engine.IsDirectoryExcluded(fullPath, relativePath));
    }

    [Fact]
    public void IsFileExcluded_RootedPattern_OnlyMatchesFullPath()
    {
        var rootedPattern = @"C:\root\nested\file.txt";
        var engine = new ExclusionEngine(filePatterns: new[] { rootedPattern });

        Assert.True(engine.IsFileExcluded(@"C:\root\nested\file.txt", @"nested\file.txt"));
        Assert.False(engine.IsFileExcluded(@"C:\root\other\file.txt", @"other\file.txt"));
    }

    [Fact]
    public void IsFileExcluded_MultiplePatterns_MatchesIfAnyPatternMatches()
    {
        var engine = new ExclusionEngine(filePatterns: new[] { "*.tmp", "*.bak", "specific.txt" });

        Assert.True(engine.IsFileExcluded(@"C:\root\a.tmp", "a.tmp"));
        Assert.True(engine.IsFileExcluded(@"C:\root\a.bak", "a.bak"));
        Assert.True(engine.IsFileExcluded(@"C:\root\specific.txt", "specific.txt"));
        Assert.False(engine.IsFileExcluded(@"C:\root\keep.txt", "keep.txt"));
    }

    [Fact]
    public void DirectoryAndFileExclusions_AreEvaluatedIndependently()
    {
        var engine = new ExclusionEngine(
            directoryPatterns: new[] { "node_modules" },
            filePatterns: new[] { "*.log" });

        Assert.True(engine.IsDirectoryExcluded(@"C:\root\node_modules", "node_modules"));
        Assert.False(engine.IsFileExcluded(@"C:\root\node_modules", "node_modules"));
        Assert.True(engine.IsFileExcluded(@"C:\root\app.log", "app.log"));
        Assert.False(engine.IsDirectoryExcluded(@"C:\root\app.log", "app.log"));
    }
}
