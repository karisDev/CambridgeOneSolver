using System;
using System.Diagnostics;
using System.Windows.Controls;
using Squirrel;

namespace CambridgeOneSolver.Views.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsAndCreditsUC.xaml
    /// </summary>
    public partial class SettingsAndCreditsUC : UserControl
    {
        public SettingsAndCreditsUC()
        {
            InitializeComponent();
            AddVersionNumber();
            CheckForUpdates();
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            IsDarkTheme.IsChecked = IsDarkTheme.IsChecked;
        }
        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo vi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ReleaseVersion.Text = $"Версия v.{ vi.FileVersion }";
        }
        private async void CheckForUpdates()
        {
            UpdateIndicator.IsIndeterminate = true;
            UpdateStatus.Text = "Проверяем обновления";
            try
            {
                using (UpdateManager manager = await UpdateManager
                    .GitHubUpdateManager(@"https://github.com/karisDev/COS_Updates"))
                {
                    var UpdateInfo = await manager.CheckForUpdate();
                    if (UpdateInfo.ReleasesToApply.Count > 0)
                    {
                        UpdateStatus.Text = "Загружается обновление";
                        await manager.UpdateApp();
                        UpdateStatus.Text = "Обновление установится после перезапуска";
                    }
                    else
                    {
                        UpdateStatus.Text = "Установлена актуальная версия";
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                UpdateStatus.Text = "Ошибка при получении актуальной версии";
            }
            UpdateIndicator.IsIndeterminate = false;
        }
        // Squirrel --releasify CambridgeOneDemo[version].nupkg
    }
}
