namespace ZipAll;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (CliOptions.LooksLikeCliInvocation(args))
        {
            NativeConsole.AttachToParentConsole();
            return CliRunner.RunAsync(args).GetAwaiter().GetResult();
        }

        ApplicationConfiguration.Initialize();
        var form = new MainForm();

        if (args.Length >= 1 && Directory.Exists(args[0]))
        {
            form.SetSourceDirectory(Path.GetFullPath(args[0]));
        }

        Application.Run(form);
        return 0;
    }
}
