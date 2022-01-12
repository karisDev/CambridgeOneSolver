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
            if ((this.DataContext is CambridgeWindowViewModel vm) && (vm.ChangeThemeCommand.CanExecute(null)))
                vm.ApplyThemeColor(AppConstants.IsThemeDark);

            Thread thread = new Thread(OnInitializedAsync);
            thread.Start();
        }

        private async void OnInitializedAsync()
        {
            Driver.Start();

            if (AppConstants.Email != "" || AppConstants.Email == null)
            {
                await Driver.LoginAsync();
            }
            else
            {
                await Driver.ListenLoginAsync();
            }
            

        }
        private void OnClosed(object sender, EventArgs e)
        {
            Driver.Quit();
            AppConstants.SaveData();
        }

        private void Close(object sender, RoutedEventArgs e) => Close();

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(AppConstants.IsThemeDark.ToString());
        }
    }
}
