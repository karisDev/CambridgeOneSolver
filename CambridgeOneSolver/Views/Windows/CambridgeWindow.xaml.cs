using CambridgeOneSolver.ViewModels;
using CambridgeOneSolver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CambridgeOneSolver.Infrastructure;

namespace CambridgeOneSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CambridgeWindow : Window
    {
        public CambridgeWindow() => InitializeComponent();
        
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                WindowState = WindowState.Normal;
                DragMove();
            }
            catch { }
        }
        private void OnInitialized(object sender, EventArgs e) => OnInitializedAsync();
        
        private async Task OnInitializedAsync()
        {
            Driver.Start();
            AppConstants.InitializeData();
            if (AppConstants.Email != "")
            {
                await Driver.LoginAsync();

            }
            Driver.ListenLoginAsync();
        }
        private void OnClosed(object sender, EventArgs e)
        {
            Driver.Quit();
            AppConstants.SaveData();
            Application.Current.Shutdown();
        }

        private void Close(object sender, RoutedEventArgs e) => Close();
    }
}
