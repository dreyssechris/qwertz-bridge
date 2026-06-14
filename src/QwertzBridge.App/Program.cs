using System.Runtime.InteropServices;
using QwertzBridge.Core.SelfTest;

namespace QwertzBridge.App;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Any(a => a.Equals("--selftest", StringComparison.OrdinalIgnoreCase)))
            return RunSelfTest();

        using var mutex = new Mutex(initiallyOwned: true, "QwertzBridge.SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
            return 0;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        using var context = new TrayApplicationContext();
        Application.Run(context);
        return 0;
    }

    private static int RunSelfTest()
    {
        // This is a WinExe; attach to the parent console so output lands in the terminal.
        AttachConsole(AttachParentProcess);

        var result = SelfTestRunner.Run();
        Console.WriteLine();
        Console.WriteLine("QWERTZ-Bridge self-test");
        Console.WriteLine("=======================");
        foreach (var line in result.Lines)
            Console.WriteLine(line);
        Console.WriteLine();
        Console.WriteLine(result.Success ? "Result: OK" : "Result: FAILED");
        return result.Success ? 0 : 1;
    }

    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);
}
