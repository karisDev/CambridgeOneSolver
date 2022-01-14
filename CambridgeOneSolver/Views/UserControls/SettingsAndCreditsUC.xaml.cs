using System;
using System.Windows.Controls;

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
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            IsDarkTheme.IsChecked = IsDarkTheme.IsChecked;
        }
        // Squirrel --releasify CambridgeOneDemo[version].nupkg
    }
}
