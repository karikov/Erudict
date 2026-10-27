using System.Runtime.InteropServices;

namespace ERuDict;

internal static class Program
{
    private const string MutexName = @"Local\ERuDict_SingleInstance";
    private const int ErrorAlreadyExists = 183;
    private const int ErrorAccessDenied = 5;

    private static nint _instanceMutex;

    [STAThread]
    private static void Main()
    {
        try
        {
            UiNative.InitializeProcess();
            UiTheme.Ensure();
            var settings = Settings.Load();
            I18n.ApplyPreference(settings.UiLanguage);

            if (!TryAcquireInstance())
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

    private static bool TryAcquireInstance()
    {
        SetLastError(0);
        _instanceMutex = CreateMutex(0, false, MutexName);
        var error = Marshal.GetLastWin32Error();
        if (_instanceMutex == 0)
        {
            AppLogger.Error($"Не удалось занять единственный экземпляр. Win32={error}.");
            return false;
        }

        if (error is ErrorAlreadyExists or ErrorAccessDenied)
        {
            CloseHandle(_instanceMutex);
            _instanceMutex = 0;
            return false;
        }

        return true;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateMutexW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateMutex(nint lpMutexAttributes, bool bInitialOwner, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint hObject);

    [DllImport("kernel32.dll")]
    private static extern void SetLastError(uint dwErrCode);
}
