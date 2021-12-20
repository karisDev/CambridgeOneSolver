using CambridgeOneSolver.ViewModels;
using CambridgeOneSolver.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CambridgeOneSolver.Infrastructure;

namespace CambridgeOneSolver
{
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
            OnInitializedAsync();

            //Call command from viewmodel     
            if ((this.DataContext is CambridgeWindowViewModel vm) && (vm.ChangeThemeCommand.CanExecute(null)))
                vm.ChangeThemeCommand.Execute(null);
        }

        private async Task OnInitializedAsync()
        {
            Driver.Start();
            AppConstants.InitializeData();
            if (AppConstants.Email != "")
            {
                await Driver.LoginAsync();
            }
            else
            {
                Driver.ListenLoginAsync();
            }
        }
        private void OnClosed(object sender, EventArgs e)
        {
            Driver.Quit();
            AppConstants.SaveData();
        }

        private void Close(object sender, RoutedEventArgs e) => Close();

        private void ExpanderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SettingsExpander.Visibility = (bool)ExpanderRadioButton.IsChecked ?
                    Visibility.Visible : Visibility.Collapsed;
            SettingsExpander.IsExpanded = (bool)ExpanderRadioButton.IsChecked;
            SettingsExpander.IsEnabled = (bool)ExpanderRadioButton.IsChecked;
            ExpanderButtonIcon.Kind = (bool)ExpanderRadioButton.IsChecked ?
                MaterialDesignThemes.Wpf.PackIconKind.ChevronDown : MaterialDesignThemes.Wpf.PackIconKind.ChevronUp;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
