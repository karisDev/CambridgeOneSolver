using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using Squirrel;
using CambridgeOneSolver.Models;

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
            //CheckForUpdates();
            SquirrelUpdateService sus = new SquirrelUpdateService(UpdateStatusText);
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
        private void UpdateStatusText(string message, bool isUpdateProcessRunning)
        {
            UpdateStatus.Text = message;
            UpdateIndicator.IsIndeterminate = isUpdateProcessRunning;
        }
        // Squirrel --releasify CambridgeOneDemo[version].nupkg
    }
}
