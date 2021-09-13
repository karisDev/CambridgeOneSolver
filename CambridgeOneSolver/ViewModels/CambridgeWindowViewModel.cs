using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CambridgeOneSolver.Infrastructure.Commands;
using CambridgeOneSolver.Models;
using CambridgeOneSolver.ViewModels.Base;

namespace CambridgeOneSolver.ViewModels
{
    class CambridgeWindowViewModel : ViewModel
    {
        #region Переменные
        #region Заголовок окна
        private string _Title = "Cambridge One Solver";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        #region Таблица Ответов
        private AnswersTable[] _AnswerGrid = { new AnswersTable()
        {
            Number = 1,
            Value = "Test"
        } } ;
        public AnswersTable[] AnswerGrid
        {
            get => _AnswerGrid;
            set => Set(ref _AnswerGrid, value);
        }
        #endregion

        #region Загрузка ответов
        private bool _LoadingAnswers = false;
        public bool LoadingAnswers
        {
            get => _LoadingAnswers;
            set => Set(ref _LoadingAnswers, value);
        }
        #endregion

        #region Видимость кнопки "Активировать"
        private Visibility _ActivateButtonVisibility = Visibility.Collapsed;
        public Visibility ActivateButtonVisibility
        {
            get => _ActivateButtonVisibility;
            set => Set(ref _ActivateButtonVisibility, value);
        }
        #endregion
        #endregion

        #region Команды
        #region CloseApplicationCommand

        public ICommand CloseApplicationCommand { get; }

        private void OnCloseApplicationCommandExecuted(object p)
        {
            Driver.Quit();
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

        #region RequestAnswersCommand

        public ICommand RequestAnswersCommand { get; }
        private void OnRequestAnswersCommandExecuted(object p)
        {
            LoadingAnswers = true;
            string DataLink = Driver.GetDataLink();
            if (DataLink == "")
            {
                ErrorMessages.NoDataURL();
            }
            else
            {
                
            }
            LoadingAnswers = false;
        }
        private bool CanRequestAnswersCommandExecute(object p) => true;

        #endregion
        #endregion

        public CambridgeWindowViewModel()
        {
            #region Команды

            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            MinimizeApplicationCommand = new LambdaCommand(OnMinimizeApplicationCommandExecuted, CanMinimizeApplicationCommandExecute);
            RequestAnswersCommand = new LambdaCommand(OnRequestAnswersCommandExecuted, CanRequestAnswersCommandExecute);
            #endregion
        }
    }
}
