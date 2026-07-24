using ZipAll.Core;

namespace ZipAll.Core.Tests;

public class WildcardMatcherTests
{
    [Theory]
    [InlineData("file.txt", "*.txt", true)]
    [InlineData("file.txt", "*.TXT", true)]
    [InlineData("file.txt", "*.png", false)]
    [InlineData("file.txt", "file.???", true)]
    [InlineData("file.txt", "file.??", false)]
    [InlineData("readme", "*", true)]
    [InlineData("a/b/c.txt", "a/*/c.txt", true)]
    [InlineData("a/b/c.txt", "a\\*\\c.txt", true)]
    [InlineData("", "*", true)]
    [InlineData("noextension", "*.txt", false)]
    public void IsMatch_EvaluatesWildcardsCaseInsensitively(string input, string pattern, bool expected)
    {
        Assert.Equal(expected, WildcardMatcher.IsMatch(input, pattern));
    }

    [Fact]
    public void IsMatch_TreatsBackslashAndForwardSlashAsEquivalent()
    {
        Assert.True(WildcardMatcher.IsMatch(@"a\b\c.txt", "a/b/c.txt"));
        Assert.True(WildcardMatcher.IsMatch("a/b/c.txt", @"a\b\c.txt"));
    }

    [Fact]
    public void IsMatch_EscapesRegexSpecialCharactersInPattern()
    {
        Assert.True(WildcardMatcher.IsMatch("a.b+c(d)", "a.b+c(d)"));
        Assert.False(WildcardMatcher.IsMatch("axbc(d)", "a.b+c(d)"));
    }
}
