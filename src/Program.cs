namespace ERuDict;

internal static class Program
{
    private const string MutexName = @"Local\ERuDict_SingleInstance";

    [STAThread]
    private static void Main()
    {
        try
        {
            UiNative.InitializeProcess();
            UiTheme.Ensure();
            var settings = Settings.Load();
            I18n.ApplyPreference(settings.UiLanguage);

            using var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
            if (!createdNew)
            {
                UiNative.Info(I18n.AlreadyRunning, I18n.AppName);
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                AppLogger.Error("Unhandled exception.", e.ExceptionObject as Exception);

            var app = new AppHost(settings);
            Autostart.Apply(settings.StartWithWindows);
            app.Run();
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to start the application.", ex);
            try
            {
                UiNative.Error(I18n.StartupFailed + "\n" + ex.Message, I18n.AppName);
            }
            catch (Exception boxError)
            {
                AppLogger.Error("Не удалось показать сообщение о сбое запуска.", boxError);
            }
        }
    }
}
