using ZipAll.Core;

namespace ZipAll;

internal sealed class CliOptions
{
    public required string SourceDirectory { get; init; }
    public required string DestinationFolder { get; init; }
    public string ArchiveName { get; init; } = "archive";
    public ZipCompressionMode CompressionMode { get; init; } = ZipCompressionMode.Deflate;
    public string? Password { get; init; }
    public List<string> ExcludeFilePatterns { get; init; } = new();
    public List<string> ExcludeDirectoryPatterns { get; init; } = new();
    public bool Quiet { get; init; }

    public string DestinationZipPath
    {
        get
        {
            var name = ArchiveName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? ArchiveName
                : ArchiveName + ".zip";
            return Path.Combine(DestinationFolder, name);
        }
    }

    public static bool LooksLikeCliInvocation(string[] args) =>
        args.Any(a => a.StartsWith('-'));

    public static bool TryParse(string[] args, out CliOptions? options, out string? error)
    {
        options = null;
        error = null;

        string? source = null;
        string? dest = null;
        var archiveName = "archive";
        var mode = ZipCompressionMode.Deflate;
        string? password = null;
        var excludeFiles = new List<string>();
        var excludeDirs = new List<string>();
        var quiet = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--source":
                    source = RequireValue(args, ref i, "--source");
                    break;
                case "--dest":
                    dest = RequireValue(args, ref i, "--dest");
                    break;
                case "--name":
                    archiveName = RequireValue(args, ref i, "--name");
                    break;
                case "--mode":
                    var modeValue = RequireValue(args, ref i, "--mode");
                    if (!TryParseMode(modeValue, out mode))
                    {
                        error = $"Invalid --mode value '{modeValue}'. Expected 'deflate' or 'stored'.";
                        return false;
                    }
                    break;
                case "--password":
                    password = RequireValue(args, ref i, "--password");
                    break;
                case "--exclude-file":
                    excludeFiles.Add(RequireValue(args, ref i, "--exclude-file"));
                    break;
                case "--exclude-dir":
                    excludeDirs.Add(RequireValue(args, ref i, "--exclude-dir"));
                    break;
                case "--quiet":
                    quiet = true;
                    break;
                default:
                    error = $"Unrecognized argument '{args[i]}'.";
                    return false;
            }
        }

        if (string.IsNullOrEmpty(source))
        {
            error = "--source is required.";
            return false;
        }

        if (string.IsNullOrEmpty(dest))
        {
            error = "--dest is required.";
            return false;
        }

        options = new CliOptions
        {
            SourceDirectory = source,
            DestinationFolder = dest,
            ArchiveName = archiveName,
            CompressionMode = mode,
            Password = password,
            ExcludeFilePatterns = excludeFiles,
            ExcludeDirectoryPatterns = excludeDirs,
            Quiet = quiet,
        };
        return true;
    }

    private static string RequireValue(string[] args, ref int index, string flagName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{flagName} requires a value.");
        }

        index++;
        return args[index];
    }

    private static bool TryParseMode(string value, out ZipCompressionMode mode)
    {
        switch (value.ToLowerInvariant())
        {
            case "deflate":
                mode = ZipCompressionMode.Deflate;
                return true;
            case "stored":
                mode = ZipCompressionMode.Stored;
                return true;
            default:
                mode = ZipCompressionMode.Deflate;
                return false;
        }
    }

    public const string HelpText = """
        ZipAll command-line mode

        Usage:
          ZipAll.exe --source <dir> --dest <dir> [options]

        Required:
          --source <dir>          Directory to compress.
          --dest <dir>             Destination folder for the .zip file.

        Options:
          --name <name>            Archive file name, without or with .zip (default: archive).
          --mode <deflate|stored>  Compression mode (default: deflate).
          --password <password>    Password-protect the archive with AES-256 encryption.
          --exclude-file <pattern> Exclude files by name/wildcard/relative path. Repeatable.
          --exclude-dir <pattern>  Exclude directories by name/wildcard/relative path. Repeatable.
          --quiet                  Suppress per-file progress output.
          --help, -h                Show this help text.

        Launching ZipAll.exe with no arguments, or with a single existing folder
        path as the only argument, opens the graphical interface instead.
        """;
}
