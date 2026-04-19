using System;
using System.Threading;
using System.Threading.Tasks;

namespace PPgram.Logging;

internal static class AppLog
{
    private static readonly Lock _sync = new();

    public static void InstallGlobalExceptionHooks()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Error("UnhandledException", "AppDomain unhandled exception.", ex);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Error("UnobservedTaskException", "Unobserved task exception.", e.Exception);
            e.SetObserved();
        };
    }

    public static void Info(string source, string message) => Write("INF", source, message, null);
    public static void Warning(string source, string message) => Write("WRN", source, message, null);
    public static void Error(string source, string message, Exception? ex = null) => Write("ERR", source, message, ex);
    private static void Write(string level, string source, string message, Exception? ex)
    {
        var timestamp = DateTimeOffset.Now.ToString("s");
        using (_sync.EnterScope())
        {
            Console.Out.WriteLine($"{timestamp} [{level}] [{source}] {message}");
            if (ex is not null) Console.Out.WriteLine(ex);
        }
    }
}
