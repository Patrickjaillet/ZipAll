using ZipAll.Core;

namespace ZipAll;

internal static class CliRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h"))
        {
            Console.WriteLine(CliOptions.HelpText);
            return 0;
        }

        if (!CliOptions.TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine($"Error: {error}");
            Console.Error.WriteLine();
            Console.Error.WriteLine(CliOptions.HelpText);
            return 2;
        }

        var opts = options!;

        if (!Directory.Exists(opts.SourceDirectory))
        {
            Console.Error.WriteLine($"Error: source directory not found: {opts.SourceDirectory}");
            return 1;
        }

        if (!Directory.Exists(opts.DestinationFolder))
        {
            Console.Error.WriteLine($"Error: destination folder not found: {opts.DestinationFolder}");
            return 1;
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var exclusions = new ExclusionEngine(opts.ExcludeDirectoryPatterns, opts.ExcludeFilePatterns);
        var progress = opts.Quiet
            ? null
            : new Progress<ArchiveFileEntry>(entry => Console.WriteLine($"  added: {entry.RelativePath}"));

        try
        {
            Console.WriteLine($"Compressing '{opts.SourceDirectory}' -> '{opts.DestinationZipPath}'...");

            var result = string.IsNullOrEmpty(opts.Password)
                ? await ArchiveWriter.CreateArchiveAsync(
                    opts.SourceDirectory, opts.DestinationZipPath, exclusions, opts.CompressionMode, progress, cts.Token)
                : await EncryptedArchiveWriter.CreateArchiveAsync(
                    opts.SourceDirectory, opts.DestinationZipPath, opts.Password, exclusions, opts.CompressionMode, progress, cts.Token);

            var verification = string.IsNullOrEmpty(opts.Password)
                ? ArchiveVerifier.Verify(opts.DestinationZipPath, result.EntryCount)
                : EncryptedArchiveVerifier.Verify(opts.DestinationZipPath, opts.Password, result.EntryCount);

            if (!verification.Success)
            {
                Console.Error.WriteLine($"Verification failed: {verification.FailureReason}");
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine($"Done: {result.EntryCount} files ({result.DeflatedEntryCount} deflated, {result.StoredEntryCount} stored)");
            Console.WriteLine($"Original size : {result.TotalBytesWritten:N0} bytes");
            Console.WriteLine($"Archive size  : {result.TotalCompressedBytes:N0} bytes ({result.CompressionRatio:P1} smaller)");
            Console.WriteLine($"Elapsed       : {result.Elapsed.TotalSeconds:F1}s");

            if (result.SkippedEntries.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"{result.SkippedEntries.Count} file(s) skipped:");
                foreach (var skipped in result.SkippedEntries)
                {
                    Console.WriteLine($"  {skipped.Path} ({skipped.Reason})");
                }
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled.");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
