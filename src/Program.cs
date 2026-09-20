namespace ERuDict;

internal static class Program
{
    private const string MutexName = @"Local\ERuDict_SingleInstance";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settings = Settings.Load();
        I18n.ApplyPreference(settings.UiLanguage);

        using var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                I18n.AlreadyRunning,
                I18n.AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => AppLogger.Error("Unhandled UI exception.", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            AppLogger.Error("Unhandled exception.", e.ExceptionObject as Exception);

        try
        {
            Autostart.Apply(settings.StartWithWindows);
            Application.Run(new TrayApp(settings));
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to start the application.", ex);
            MessageBox.Show(I18n.StartupFailed + "\n" + ex.Message, I18n.AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
