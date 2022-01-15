using System.Diagnostics;

namespace CambridgeOneSolver.Infrastructure
{
    class AppConstants
    {
        public readonly static string Version = GetApplicationVersion();
        public static string Email = Properties.Settings.Default.Email;
        public static string Password = Properties.Settings.Default.Password;
        public static bool IsThemeDark = Properties.Settings.Default.IsThemeDark;
        public static int AnswersFontSize = Properties.Settings.Default.AnswersFontSize;
        public static bool FirstRun = Properties.Settings.Default.FirstRun;
        public static void SaveData()
        {
            Properties.Settings.Default.Email = Email;
            Properties.Settings.Default.Password = Password;
            Properties.Settings.Default.IsThemeDark = IsThemeDark;
            Properties.Settings.Default.AnswersFontSize = AnswersFontSize;
            Properties.Settings.Default.FirstRun = FirstRun;
            Properties.Settings.Default.Save();
        }
        private static string GetApplicationVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return vi.FileVersion;
        }
    }
}
