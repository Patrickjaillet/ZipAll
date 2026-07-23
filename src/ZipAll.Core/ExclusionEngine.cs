namespace ZipAll.Core;

public sealed class ExclusionEngine
{
    private readonly List<string> _directoryPatterns;
    private readonly List<string> _filePatterns;

    public ExclusionEngine(IEnumerable<string>? directoryPatterns = null, IEnumerable<string>? filePatterns = null)
    {
        _directoryPatterns = directoryPatterns?.ToList() ?? new List<string>();
        _filePatterns = filePatterns?.ToList() ?? new List<string>();
    }

    public bool IsDirectoryExcluded(string fullPath, string relativePath) =>
        IsExcluded(fullPath, relativePath, _directoryPatterns);

    public bool IsFileExcluded(string fullPath, string relativePath) =>
        IsExcluded(fullPath, relativePath, _filePatterns);

    private static bool IsExcluded(string fullPath, string relativePath, List<string> patterns)
    {
        if (patterns.Count == 0)
        {
            return false;
        }

        var name = Path.GetFileName(relativePath);

        foreach (var pattern in patterns)
        {
            if (Path.IsPathRooted(pattern))
            {
                if (WildcardMatcher.IsMatch(fullPath, pattern))
                {
                    return true;
                }
            }
            else
            {
                if (WildcardMatcher.IsMatch(name, pattern) || WildcardMatcher.IsMatch(relativePath, pattern))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
