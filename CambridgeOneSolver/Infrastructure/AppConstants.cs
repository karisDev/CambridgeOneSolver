namespace CambridgeOneSolver.Infrastructure
{
    class AppConstants
    {
        public readonly static string Version = "1";
        public static string Email;
        public static string Password;
        public static bool IsThemeDark = false;
        public static void InitializeData()
        {
            Email = Properties.Settings.Default.Email;
            Password = Properties.Settings.Default.Password;
            IsThemeDark = Properties.Settings.Default.IsThemeDark;
        }
        public static void SaveData()
        {
            Properties.Settings.Default.Email = Email;
            Properties.Settings.Default.Password = Password;
            Properties.Settings.Default.IsThemeDark = IsThemeDark;
            Properties.Settings.Default.Save();
        }
    }
}
