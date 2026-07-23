namespace ZipAll.Core;

public readonly record struct VerificationResult(bool Success, int ActualEntryCount, int ExpectedEntryCount, string? FailureReason);
