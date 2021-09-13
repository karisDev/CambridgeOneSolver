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
        public CambridgeWindow()
        {
            InitializeComponent();
        }
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                WindowState = WindowState.Normal;
                DragMove();
            }
            catch { }
        }
        private void OnInitialized(object sender, EventArgs e)
        {
            _OnInitializedAsync();
        }
        private async Task _OnInitializedAsync()
        {
            Driver.Start();
            Constants.InitializeData();
            if (Constants.Email != "")
            {
                await Driver.LoginAsync();

            }
            // InputLogin
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Driver.ListenLoginAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
