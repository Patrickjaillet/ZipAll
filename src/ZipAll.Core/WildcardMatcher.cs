using System.Text.RegularExpressions;

namespace ZipAll.Core;

public static class WildcardMatcher
{
    public static bool IsMatch(string input, string pattern)
    {
        var normalizedInput = NormalizeSeparators(input);
        var normalizedPattern = NormalizeSeparators(pattern);
        var regexPattern = "^" + Regex.Escape(normalizedPattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".") + "$";

        return Regex.IsMatch(normalizedInput, regexPattern, RegexOptions.IgnoreCase);
    }

    private static string NormalizeSeparators(string path) => path.Replace('\\', '/');
}
