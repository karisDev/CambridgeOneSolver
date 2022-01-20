using CambridgeOneSolver.ViewModels;
using CambridgeOneSolver.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CambridgeOneSolver.Infrastructure;
using System.Windows.Shapes;
using System.Threading;

namespace CambridgeOneSolver.Views.Windows
{
    /// <summary>
    /// Interaction logic for CambridgeOneWindow.xaml
    /// </summary>
    public partial class CambridgeOneWindow : Window
    {
        // моих навыков не хватает для переноса всех функций в viewmodel. сделайте пул если вам удастся
        public CambridgeOneWindow()
        {
            InitializeComponent();
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    var point = System.Windows.Forms.Control.MousePosition;
                    Left = point.X - (Width / 2);
                    Top = point.Y - 10;
                }

                DragMove();
            }
            catch { }
        }
        private void OnInitialized(object sender, EventArgs e)
        {
            if (AppConstants.FirstRun)
            {
                AppConstants.FirstRun = false;
                Application.Current.Shutdown();
            }

            if ((this.DataContext is CambridgeWindowViewModel vm) && (vm.ChangeThemeCommand.CanExecute(null)))
                vm.ApplyThemeColor(AppConstants.IsThemeDark);
        }
        private void HomeClick(object sender, RoutedEventArgs e)
        {
            MainTransitioner.SelectedIndex = 0;
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            MainTransitioner.SelectedIndex = 1;
        }

        private void DonatorsClick(object sender, RoutedEventArgs e)
        {
            MainTransitioner.SelectedIndex = 2;
        }
    }
}
