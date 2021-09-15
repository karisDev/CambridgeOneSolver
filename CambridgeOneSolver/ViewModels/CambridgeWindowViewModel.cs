using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CambridgeOneSolver.Infrastructure;
using CambridgeOneSolver.Infrastructure.Commands;
using CambridgeOneSolver.Models;
using CambridgeOneSolver.ViewModels.Base;
using MaterialDesignThemes.Wpf;

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
        private AnswersTable[] _AnswerGrid;
        public AnswersTable[] AnswerGrid
        {
            get => _AnswerGrid;
            set => Set(ref _AnswerGrid, value);
        }
        #endregion

        #region Таблица Ответов
        private bool _ShowLoginBar = false;
        public bool ShowLoginBar
        {
            get => _ShowLoginBar;
            set => Set(ref _ShowLoginBar, value);
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

        #region Закрепление окна
        private bool _IsOnTop = true;
        public bool IsOnTop
        {
            get => _IsOnTop;
            set => Set(ref _IsOnTop, value);
        }
        #endregion
        #endregion

        #region Команды
        #region CloseApplicationCommand

        public ICommand CloseApplicationCommand { get; }

        private void OnCloseApplicationCommandExecuted(object p)
        {
            Driver.Quit();
            AppConstants.SaveData();
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
        private async void OnRequestAnswersCommandExecuted(object p)
        {
            LoadingAnswers = true;
            string DataLink = Driver.GetDataLink();
            if (DataLink == "")
            {
                ErrorMessages.NoDataURL();
            }
            else
            {
                try
                {
                    ServerRequests sr = await ServerRequests.Asnwers(DataLink, AppConstants.Email, AppConstants.Version);
                    if (sr.DisplayMessage != " ") MessageBox.Show(sr.DisplayMessage);
                    if (sr.Success == false) ActivateButtonVisibility = Visibility.Visible;
                    DisplayAnswers(sr.Data);
                }
                catch
                {
                    ErrorMessages.ApiServerConnectionError();
                }

            }
            LoadingAnswers = false;
        }
        private bool CanRequestAnswersCommandExecute(object p) => true;

        #endregion

        #region ChangeThemeCommand

        public ICommand ChangeThemeCommand { get; }
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private void OnChangeThemeCommandExecuted(object p)
        {
            Infrastructure.AppConstants.IsThemeDark = !Infrastructure.AppConstants.IsThemeDark;
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = Infrastructure.AppConstants.IsThemeDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
        }
        private bool CanChangeThemeCommandExecute(object p) => true;
        #endregion

        #region VisitBuyPageCommand

        public ICommand VisitBuyPageCommand { get; }

        private void OnVisitBuyPageCommandExecuted(object p)
        {
            IsOnTop = false;
            System.Diagnostics.Process.Start("https://sobe.ru/na/cambridgeonesolver");
        }
        private bool CanVisitBuyPageCommandExecute(object p) => true;
        #endregion
        #endregion

        #region Функции
        public void DisplayAnswers(string[] answers)
        {
            if (answers.Length > 0)
            {
                AnswersTable[] at = new AnswersTable[answers.Length];

                for (int i = 0; i < answers.Length; i++)
                {
                    at[i] = new AnswersTable() { Number = i + 1, Value = answers[i].Replace("\\", "") };
                }
                AnswerGrid = at;
            }
            else ErrorMessages.NoAnswersRecieved();
        }
        #endregion
        public CambridgeWindowViewModel()
        {
            #region Команды

            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            MinimizeApplicationCommand = new LambdaCommand(OnMinimizeApplicationCommandExecuted, CanMinimizeApplicationCommandExecute);
            RequestAnswersCommand = new LambdaCommand(OnRequestAnswersCommandExecuted, CanRequestAnswersCommandExecute);
            ChangeThemeCommand = new LambdaCommand(OnChangeThemeCommandExecuted, CanChangeThemeCommandExecute);
            VisitBuyPageCommand = new LambdaCommand(OnVisitBuyPageCommandExecuted, CanVisitBuyPageCommandExecute);
            #endregion
        }
    }
}
