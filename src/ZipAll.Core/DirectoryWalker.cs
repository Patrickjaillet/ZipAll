namespace ZipAll.Core;

public static class DirectoryWalker
{
    private static readonly EnumerationOptions TraversalOptions = new()
    {
        AttributesToSkip = FileAttributes.None,
        RecurseSubdirectories = false,
        IgnoreInaccessible = false,
        MatchType = MatchType.Simple,
        ReturnSpecialDirectories = false
    };

    public static IEnumerable<ArchiveFileEntry> EnumerateFiles(
        string rootDirectory,
        ExclusionEngine? exclusions = null,
        ICollection<SkippedEntry>? skipped = null)
    {
        var root = Path.GetFullPath(rootDirectory);

        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException(root);
        }

        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(root);

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();

            List<string> filesInDirectory;
            List<string> subdirectories;

            try
            {
                filesInDirectory = Directory.EnumerateFiles(currentDirectory, "*", TraversalOptions).ToList();
                subdirectories = Directory.EnumerateDirectories(currentDirectory, "*", TraversalOptions).ToList();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                skipped?.Add(new SkippedEntry(currentDirectory, ex.Message));
                continue;
            }

            foreach (var filePath in filesInDirectory)
            {
                var relativePath = Path.GetRelativePath(root, filePath);

                if (exclusions is not null && exclusions.IsFileExcluded(filePath, relativePath))
                {
                    continue;
                }

                FileAttributes attributes;

                try
                {
                    attributes = File.GetAttributes(filePath);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    skipped?.Add(new SkippedEntry(filePath, ex.Message));
                    continue;
                }

                yield return new ArchiveFileEntry(filePath, relativePath, attributes);
            }

            foreach (var directoryPath in subdirectories)
            {
                var relativeDirectoryPath = Path.GetRelativePath(root, directoryPath);

                if (exclusions is not null && exclusions.IsDirectoryExcluded(directoryPath, relativeDirectoryPath))
                {
                    continue;
                }

                pendingDirectories.Push(directoryPath);
            }
        }
    }
}
