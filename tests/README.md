# Tests

## `ZipAll.Core.Tests`
xUnit unit test project covering `ZipAll.Core`: `WildcardMatcher`, `ExclusionEngine`, `DirectoryWalker`, and `ArchiveWriter`/`ArchiveVerifier`. Includes edge cases (empty folders, deeply nested trees, special/Unicode characters in file names, large flat trees) and an exclusion pattern test suite.

Run with:
```
dotnet test tests/ZipAll.Core.Tests/ZipAll.Core.Tests.csproj
```

## `ExclusionHarness`
Console harness that builds a realistic project-like tree (120-level-deep chain, `node_modules`/`bin`/`obj`, log/tmp files, an absolute-path exclusion) and asserts the exclusion engine and archive writer agree on what gets filtered out. Complements the xUnit exclusion tests with a scenario closer to a real project structure.

## `ManualHarness`
Console harness exercising a full folder → `.zip` round trip against a real `ZipArchive` reader: nested folders, an empty folder, Unicode file names, read-only/hidden/system attributes, a path longer than `MAX_PATH`, and a locked file held open during compression to verify graceful skip-and-continue behavior.

Both harnesses can be run with `dotnet run --project tests/<HarnessName>/<HarnessName>.csproj` and exit with a non-zero code if any check fails.

## Manual regression checklist
See [`REGRESSION_CHECKLIST.md`](REGRESSION_CHECKLIST.md) for the manual pass to run against a built installer before every release.
