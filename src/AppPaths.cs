namespace ERuDict;

internal static class AppPaths
{
    public static string Directory
    {
        get
        {
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe))
            {
                var dir = Path.GetDirectoryName(exe);
                if (!string.IsNullOrEmpty(dir))
                {
                    return dir;
                }
            }

            return AppContext.BaseDirectory;
        }
    }
}
