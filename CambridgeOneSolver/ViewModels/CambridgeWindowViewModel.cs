using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CambridgeOneSolver.Infrastructure.Commands;
using CambridgeOneSolver.ViewModels.Base;

namespace CambridgeOneSolver.ViewModels
{
    class CambridgeWindowViewModel : ViewModel
    {
        #region Заголовок окна
        private string _Title = "Cambridge One Solver";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        #region Команды
        #region CloseApplicationCommand

        public ICommand CloseApplicationCommand { get; }

        private void OnCloseApplicationCommandExecuted(object p)
        {
            // CambridgeWindow.Driver.Quit()
            Application.Current.Shutdown();
        }
        private bool CanCloseApplicationCommandExecute(object p) => true;
        #endregion
        #region MinimizeApplicationCommand
        public ICommand MinimizeApplicationCommand { get; }
        private void OnMinimizeApplicationCommandExecuted(object p)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        private bool CanMinimizeApplicationCommandExecute(object p) => true;
        #endregion
        #endregion
        public CambridgeWindowViewModel()
        {
            #region Команды

            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            MinimizeApplicationCommand = new LambdaCommand(OnMinimizeApplicationCommandExecuted, CanMinimizeApplicationCommandExecute);
            #endregion
        }
    }
}
