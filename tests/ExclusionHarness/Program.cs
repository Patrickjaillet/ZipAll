using ZipAll.Core;

var workDir = Path.Combine(Path.GetTempPath(), "ZipAllExclusionHarness_" + Guid.NewGuid().ToString("N"));
var sourceDir = Path.Combine(workDir, "source");
var zipPath = Path.Combine(workDir, "output.zip");

var allExpectedFailures = new List<string>();

Directory.CreateDirectory(sourceDir);

const int deepLevels = 120;
var deepPath = sourceDir;
for (var i = 1; i <= deepLevels; i++)
{
    deepPath = Path.Combine(deepPath, $"level-{i}");
    Directory.CreateDirectory(deepPath);
    File.WriteAllText(Path.Combine(deepPath, $"file-at-level-{i}.txt"), $"content at level {i}");
}

var nodeModules = Path.Combine(sourceDir, "src", "node_modules");
Directory.CreateDirectory(Path.Combine(nodeModules, "some-package", "lib"));
File.WriteAllText(Path.Combine(nodeModules, "some-package", "index.js"), "module.exports = {};");
File.WriteAllText(Path.Combine(nodeModules, "some-package", "lib", "deep.js"), "console.log('deep');");

var binDir = Path.Combine(sourceDir, "src", "bin");
Directory.CreateDirectory(binDir);
File.WriteAllText(Path.Combine(binDir, "app.dll"), "fake binary");

var objDir = Path.Combine(sourceDir, "src", "obj");
Directory.CreateDirectory(objDir);
File.WriteAllText(Path.Combine(objDir, "temp.obj"), "fake object file");

var srcDir = Path.Combine(sourceDir, "src");
File.WriteAllText(Path.Combine(srcDir, "Program.cs"), "class Program {}");
File.WriteAllText(Path.Combine(srcDir, "build.log"), "build output");
File.WriteAllText(Path.Combine(srcDir, "scratch.tmp"), "scratch data");
File.WriteAllText(Path.Combine(srcDir, "Thumbs.db"), "windows thumbnail cache");

var absoluteExcludedDir = Path.Combine(sourceDir, "vendor-absolute-excluded");
Directory.CreateDirectory(absoluteExcludedDir);
File.WriteAllText(Path.Combine(absoluteExcludedDir, "vendor-file.txt"), "should be excluded by absolute path rule");

var directoryPatterns = new[] { "node_modules", "bin", "obj", absoluteExcludedDir };
var filePatterns = new[] { "*.log", "*.tmp", "Thumbs.db" };
var exclusions = new ExclusionEngine(directoryPatterns, filePatterns);

var entries = DirectoryWalker.EnumerateFiles(sourceDir, exclusions).ToList();
var relativePaths = entries.Select(e => e.RelativePath.Replace('\\', '/')).ToHashSet();

Check("deep chain file at level 1 is present", relativePaths.Contains($"level-1/file-at-level-1.txt"));
Check($"deep chain file at level {deepLevels} is present", relativePaths.Any(p => p.EndsWith($"file-at-level-{deepLevels}.txt")));
Check("src/Program.cs is present", relativePaths.Contains("src/Program.cs"));

Check("no node_modules content leaked through", relativePaths.All(p => !p.Contains("node_modules")));
Check("no bin content leaked through", relativePaths.All(p => !p.Split('/').Contains("bin")));
Check("no obj content leaked through", relativePaths.All(p => !p.Split('/').Contains("obj")));
Check("no *.log file leaked through", relativePaths.All(p => !p.EndsWith(".log")));
Check("no *.tmp file leaked through", relativePaths.All(p => !p.EndsWith(".tmp")));
Check("Thumbs.db excluded", relativePaths.All(p => !p.EndsWith("Thumbs.db")));
Check("absolute-path excluded directory not present", relativePaths.All(p => !p.Contains("vendor-absolute-excluded")));

var expectedEntryCount = entries.Count;
Console.WriteLine();
Console.WriteLine($"Entries after exclusion filtering: {expectedEntryCount}");

var result = await ArchiveWriter.CreateArchiveAsync(sourceDir, zipPath, exclusions: exclusions);
Check("archive entry count matches filtered walker count", result.EntryCount == expectedEntryCount);

var verification = ArchiveVerifier.Verify(zipPath, expectedEntryCount);
Check("archive verification passed", verification.Success);

Directory.Delete(workDir, recursive: true);

Console.WriteLine();
if (allExpectedFailures.Count == 0)
{
    Console.WriteLine("ALL CHECKS PASSED");
    return 0;
}

Console.WriteLine($"{allExpectedFailures.Count} CHECK(S) FAILED:");
foreach (var failure in allExpectedFailures)
{
    Console.WriteLine($"  - {failure}");
}
return 1;

void Check(string description, bool condition)
{
    Console.WriteLine($"  [{(condition ? "PASS" : "FAIL")}] {description}");
    if (!condition)
    {
        allExpectedFailures.Add(description);
    }
}
