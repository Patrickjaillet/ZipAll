namespace ZipAll.Core;

public readonly record struct ArchiveResult(
    int EntryCount,
    long TotalBytesWritten,
    long TotalCompressedBytes,
    int StoredEntryCount,
    int DeflatedEntryCount,
    TimeSpan Elapsed,
    IReadOnlyList<SkippedEntry> SkippedEntries)
{
    public double CompressionRatio =>
        TotalBytesWritten <= 0 ? 0d : 1d - ((double)TotalCompressedBytes / TotalBytesWritten);
}
