using Avalonia;
using Serilog;
using System;

namespace KWRP.Avalonia.Frontend
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // ロギングの設定
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File("logs/kwrpLog.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes: 5242880,
                    retainedFileCountLimit: 30)
                .CreateLogger();

            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                              $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff")}|Fatal|システムに予期せぬエラーが発生しました\n" +
                              $"続けて発生する場合は開発者に報告してください。\n" +
                              $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseR3();
    }
}
