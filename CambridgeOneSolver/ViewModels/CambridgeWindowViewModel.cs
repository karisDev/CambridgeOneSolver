using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        public DriverRework driver;
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

        #region Загрузка ответов
        private bool _LoadingAnswers = false;
        public bool LoadingAnswers
        {
            get => _LoadingAnswers;
            set => Set(ref _LoadingAnswers, value);
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

        #region Темная ли тема
        private bool _IsThemeDark = AppConstants.IsThemeDark;
        public bool IsThemeDark
        {
            get => _IsThemeDark;
            set
            {
                Set(ref _IsThemeDark, value);
                AppConstants.IsThemeDark = _IsThemeDark;
                ApplyThemeColor(_IsThemeDark);
            }
        }
        #endregion

        #region Авто ввод
        private bool _IsAutoFill = AppConstants.IsAutoFillEnabled;
        public bool IsAutoFill
        {
            get => _IsAutoFill;
            set => Set(ref _IsAutoFill, value);
        }
        #endregion

        #region Размер шрифта

        private int _AnswersFontSize = AppConstants.AnswersFontSize;
        public int AnswersFontSize
        {
            get => _AnswersFontSize;
            set
            {
                Set(ref _AnswersFontSize, value);
                AppConstants.AnswersFontSize = _AnswersFontSize;
            }
        }

        #endregion
        #endregion

        #region Команды
        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }

        private void OnCloseApplicationCommandExecuted(object p)
        {
            if (driver != null)
                driver.Close();
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
            string DataLink = driver.RetrieveDataLink();
            if (DataLink == null)
                ErrorMessages.NoDataURL();
            else
            {
                try
                {
                    ServerRequests sr = await ServerRequests.Asnwers(DataLink, AppConstants.Email, AppConstants.Version);
                    if (sr.DisplayMessage != null)
                        MessageBox.Show(sr.DisplayMessage);

                    if (sr.Data == null)
                    {
                        ErrorMessages.NoAnswersRecieved();
                    }
                    else
                    {
                        DisplayAnswers(sr.Data);
                        if (_IsAutoFill)
                        {
                            Thread thread = new Thread(() => driver.FillAnswersMachine(sr.Data, sr.TasksTag));
                            thread.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message);
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
            ApplyThemeColor(IsThemeDark);
/*            Infrastructure.AppConstants.IsThemeDark = !Infrastructure.AppConstants.IsThemeDark;
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = Infrastructure.AppConstants.IsThemeDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
*/        }
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

        #region DeleteSavedDataCommand
        public ICommand DeleteSavedDataCommand { get; }

        private void OnDeleteSavedDataCommandExecuted(object p)
        {
            try
            {
                driver.Close();
            } catch { }
            
            Process.Start(Application.ResourceAssembly.Location);
            AppConstants.Email = "";
            AppConstants.SaveData();
            Application.Current.Shutdown();
        }
        private bool CanDeleteSavedDataCommandExecute(object p) => true;
        #endregion

        #region CheckForUpdatesCommand

        #endregion
        #endregion

        #region Функции
        public void DisplayAnswers(string[] answers)
        {
            if (answers != null)
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

        public void ApplyThemeColor(bool isDark)
        {
            //Infrastructure.AppConstants.IsThemeDark = isDark;
            ITheme theme = _paletteHelper.GetTheme();
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
        }

        #endregion

        #region Для драйвера
        private void OnInitialize()
        {
            driver = new DriverRework(MessageBox.Show);
            
            if (AppConstants.Email != "" || AppConstants.Email == null)
            {
                driver.FillLoginPage();
            }
            else
            {
                driver.DetectLoginPage();
            }
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
            DeleteSavedDataCommand = new LambdaCommand(OnDeleteSavedDataCommandExecuted, CanDeleteSavedDataCommandExecute);
            #endregion
            Thread thread = new Thread(OnInitialize);
            thread.Start();

        }
    }
}
