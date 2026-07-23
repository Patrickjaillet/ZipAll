namespace ZipAll.Core;

public readonly record struct ArchiveFileEntry(string FullPath, string RelativePath, FileAttributes Attributes);
