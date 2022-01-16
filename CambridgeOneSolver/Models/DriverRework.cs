using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CambridgeOneSolver.Infrastructure;
using System.Windows;

namespace CambridgeOneSolver.Models
{
    class DriverRework
    {
        private readonly ChromeDriver driver;
        private readonly Func<string, MessageBoxResult> PrintErrorMessage;
        private bool IsRunning = false;
        private IWebElement CurrentContentWrap;
        private int ContentWrapID;
        public DriverRework(Func<string, MessageBoxResult> ErrorMessagesNotifier)
        {
            PrintErrorMessage = ErrorMessagesNotifier;
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += new UnhandledExceptionEventHandler(ErrorHandler);

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            try
            {
                driver = new ChromeDriver(driverService, new ChromeOptions())
                {
                    Url = "https://www.cambridgeone.org/login"
                };

                IsRunning = true;
            }
            catch (DriverServiceNotFoundException)
            {
                PrintErrorMessage("Произошел конфликт с версией браузера Google Chrome. Если у вас последняя версия браузера - сообщите в группу.");
            }
        }
        private void ErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            PrintErrorMessage($"Была обнаружена неизвестная ошибка. Если хотите помочь исправить ее, то напишите в группу.\nТекст ошибки: {e.Message}\nStackTrace: {e.StackTrace}");
        }
        public void Close()
        {
            try
            {
                driver.Quit();
                IsRunning = false;
            }
            catch (WebDriverException)
            {
                PrintErrorMessage("Программа сама в состоянии закрыть браузер. Вам не нужно его закрывать :)");
            }
        }
        private bool WaitForElement(string xpath, int IterationLimit = 9999) // ожидание Limit * 0.2 секунд
        {
            int Iterations = 0;
            while (CheckForElement(xpath) == false && ++Iterations < IterationLimit)
            {
                Task.Delay(100).Wait();
            }
            return Iterations < IterationLimit;
        }
        private bool CheckForElement(string xpath)
        {
            try
            {
                driver.FindElement(By.XPath(xpath));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        private bool ClickMultipleTimes(string xpath, int IterationLimit = 9999)
        {
            int Iterations = 0;
            while (++Iterations < IterationLimit)
            {
                try
                {
                    driver.FindElement(By.XPath(xpath)).Click();
                    return true;
                }
                catch (Exception ex)
                {
                    Type exType = ex.GetType();
                    if (exType == typeof(ElementNotInteractableException) ||
                        exType == typeof(NoSuchElementException))
                    {
                        Task.Delay(100).Wait();
                    }
                    else
                    {
                        PrintErrorMessage($"Неизвестная ошибка при попытке нажать на \"{xpath}\". Скорее всего кнопки нет на экране или ответы не совпадают.");
                        return false; ;
                    }
                }
            }
            return false;
        }
        private bool ClickMultipleTimes(IWebElement button, int IterationLimit = 9999)
        {
            int Iterations = 0;
            while (++Iterations < IterationLimit)
            {
                try
                {
                    button.Click();
                    return true;
                }
                catch (Exception ex)
                {
                    Type exType = ex.GetType();
                    if (exType == typeof(ElementNotInteractableException) ||
                        exType == typeof(NoSuchElementException))
                    {
                        Task.Delay(100).Wait();
                    }
                    else
                    {
                        PrintErrorMessage($"Бот не смог найти кнопку");
                        return false;
                    }
                }
            }
            return false;
        }
        public string RetrieveDataLink()
        {
            if (IsRunning)
            {
                try
                {
                    driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                foreach (IWebElement webElement in driver.FindElements(By.TagName("script")))
                {
                    if (webElement.GetAttribute("src").Contains("data.js"))
                    {
                        string result = webElement.GetAttribute("src");
                        driver.SwitchTo().DefaultContent();
                        return result;
                    }
                }
            }
            driver.SwitchTo().DefaultContent();
            return null;
        }
        public bool IsClosed()
        {
            try
            {
                string temp = driver.Url;
                return false;
            }
            catch (WebDriverException)
            {
                return true;
            }
        }
        public async Task DetectLoginPage()
        {
            string url = "https://www.cambridgeone.org/login";
            while (IsRunning)
            {
                if (driver.Url == url)
                {
                    await SaveLoginData();
                }
                else
                {
                    await Task.Delay(10000);
                }
            }
        }
        public async Task SaveLoginData()
        {
            string url = "https://www.cambridgeone.org/login";
            while (IsRunning && driver.Url == url)
            {
                try
                {
                    AppConstants.Email = driver.FindElement(By.XPath("//input[@id=\"gigya-loginID-56269462240752180\"]")).GetAttribute("value");
                    AppConstants.Password = driver.FindElement(By.XPath("//input[@id=\"gigya-loginID-56269462240752180\"]")).GetAttribute("value");
                }
                catch { }
                await Task.Delay(400);
            }
        }
        public void FillLoginPage()
        {
            WaitForElement("//*[@id=\"gigya-loginID-56269462240752180\"]");
            if (IsRunning)
            {
                driver.FindElement(By.Id("gigya-loginID-56269462240752180")).SendKeys(AppConstants.Email);
                driver.FindElement(By.Id("gigya-password-56383998600152700")).SendKeys(AppConstants.Password);
                driver.FindElement(By.XPath("//input[@value=\"Log in\"]")).Click();
            }
        }
        public void FillAnswersMachine(string[] AnswersArray, int[] TasksTag)
        {
            driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));

            // если нет вопросов, то просто нажимаем кнопку Next
            bool IsPresentation = CheckIfPresentation(TasksTag);
            if (IsPresentation)
            {
                SolvePresentation();
                return;
            }

            CurrentContentWrap = GetActiveContentWrap();
            if (CurrentContentWrap == null)
                PrintErrorMessage("Найден какой-то неизвестный CSS селектор и программа не может вводить ответы. Напишите в группу, чтобы я исправил проблему.");
            else
            {
                do
                {
                    // получаем код задания на экране и его номер по счету
                    CurrentContentWrap = GetActiveContentWrap();
                    ContentWrapID = int.Parse(CurrentContentWrap.GetAttribute("id").Split('_').Last());

                    // ответов бывает меньше, чем заданий (из-за презентаций)

                    if (AnswersArray.Length <= ContentWrapID)
                    {
                        SolveTaskByTag(null, TasksTag[ContentWrapID]);
                    }
                    else
                    {
                        SolveTaskByTag(AnswersArray[ContentWrapID], TasksTag[ContentWrapID]);
                    }

                } while (ContentWrapID != TasksTag.Length - 1);
            }

            driver.SwitchTo().DefaultContent();
        }
        private bool CheckIfPresentation(int[] TasksTag)
        {
            foreach (int TaskTag in TasksTag)
            {
                if (TaskTag != 5)
                    return false;
            }
            return true;
        }
        private void SolvePresentation()
        {
            string NextButtonXPath = "//a[@title=\"Next\"]";
            driver.SwitchTo().DefaultContent();
            while (true)
            {
                Task.Delay(2500).Wait();
                if (!ClickMultipleTimes(NextButtonXPath, 10))
                {
                    return;
                }
            }
        }
        private IWebElement GetActiveContentWrap()
        {
            try
            {
                // бывает три разных типа content_wrap, проверяем по популярности
                if (CheckForElement("//section[@style=\"display: flex; top: 0px;\"]"))
                {
                    return driver.FindElement(
                        By.XPath("//section[@style=\"display: flex; top: 0px;\"]"));
                }
                else if (CheckForElement("//section[@style=\"display: flex;\"]"))
                {
                    return driver.FindElement(
                        By.XPath("//section[@style=\"display: flex;\"]"));
                }
                else
                {
                    return driver.FindElement(
                        By.XPath("//div[@class=\"content-wrap at-presentation\"]"));
                }////*[@id="content_wrap_0"]
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
        private void NavigateToNextTask()
        {
            driver.SwitchTo().DefaultContent();

            string CheckButtonXPath = "//a[@class=\"btn green-btn\"]";
            string NextButtonXPath = "//a[@title=\"Next\"]";

            if (WaitForElement(CheckButtonXPath, 10))
            {
                ClickMultipleTimes(CheckButtonXPath);
                ClickMultipleTimes(NextButtonXPath);
            }
            else if (WaitForElement(NextButtonXPath, 10))
            {
                ClickMultipleTimes(NextButtonXPath);
            }
            driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
        }

        /*
         * TaskTag обозначает типы заданий в кэмбридже, его получают от сервера.
         * Таблица типов:
         * 0 - Текстовое поле "Input:Completion:Text gap"
         * 1 - Радио-кнопка "Identify:Select:Radiobutton"
         * 2 - Комбобокс "Identify:Select:Dropdown"
         * 3 - Перетаскивание элементов "Order:Match:Text gap"
         * 4 - Чекбокс "Identify:Select:Checkbox"
         * 5 - Информация без задания "Present:Present:Present"
         * 6 - Перетаскивание элементов, но особенное "Order:Sort:Sorting"
         * 7 - Запись голоса "recorder"
         * -1 - Неизвестный тип задания, на всякий
         */
        private void SolveTaskByTag(string Answer, int TaskTag)
        {
            switch (TaskTag)
            {
                case 0:
                    TextGapHelper(Answer);
                    NavigateToNextTask();
                    break;
                case 1:
                    RadioButtonHelper(Answer);
                    NavigateToNextTask();
                    break;
                case 2:
                    ComboBoxHelper(Answer);
                    NavigateToNextTask();
                    break;
                case 3:
                    DragAndDropHelper(Answer);
                    NavigateToNextTask();
                    break;
                case 4:
                    CheckBoxHelper(Answer);
                    NavigateToNextTask();
                    break;
                case 5:
                    PresentationHelper();
                    break;
                case 6:
                    AdvancedDragAndDropHelper(Answer);
                    NavigateToNextTask();
                    break;
                default:
                    PrintErrorMessage("Задание выполняется вручную. Так как все работает в цикле это окно не закроется пока вы не перейдете к след. заданию.");
                    break;
            }
        }
        private void TextGapHelper(string Answer)
        {
            Task.Delay(300).Wait();
            CurrentContentWrap.FindElement(By.XPath($"//section[contains(@style,\"flex\")]//input")).SendKeys(Answer.Replace('\n', '\t'));
        }
        private void RadioButtonHelper(string Answer)
        {
            var buttons = CurrentContentWrap.FindElements(By.XPath("//span"));
            foreach (IWebElement button in buttons)
            {
                if (button.GetAttribute("innerHTML") == Answer)
                {
                    ClickMultipleTimes(button, 4);
                    return;
                }
            }
        }
        private void PresentationHelper()
        {
            driver.SwitchTo().DefaultContent();

            string NextButtonXPath = "//a[@title=\"Next\"]";
            ClickMultipleTimes(NextButtonXPath, 4);

            driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));
        }
        private void ComboBoxHelper(string Answer)
        {
            Task.Delay(300).Wait();
            string[] SplittedAnswers = Answer.Split('\n');

            var ComboBoxes = CurrentContentWrap.FindElements(
                By.XPath($"//section[@id=\"content_wrap_{ContentWrapID}\"]//span[@class=\"label drop-label\"]"));
            int CurrentComboBox = 0;
            foreach (IWebElement ComboBox in ComboBoxes)
            {
                ClickMultipleTimes(ComboBox, 5);

                var ComboBoxItems = CurrentContentWrap.FindElements(By.XPath("//li[@data-model-id]"));
                foreach (IWebElement ComboBoxItem in ComboBoxItems)
                {
                    if (ComboBoxItem.GetAttribute("innerHTML") == SplittedAnswers[CurrentComboBox])
                    {
                        ClickMultipleTimes(ComboBoxItem, 5);
                        break;
                    }
                }

                CurrentComboBox++;
            }
        }
        private void DragAndDropHelper(string Answer)
        {
            Task.Delay(300).Wait();
            List<string> SplittedAnswers = Answer.Split('\n').ToList();

            while (SplittedAnswers.Count != 0)
            {
                Task.Delay(250).Wait();
                var DragAndDropItems = CurrentContentWrap.FindElements(By.XPath(
                    $"//section[@id=\"content_wrap_{ContentWrapID}\"]//div[@class=\"pool ui-droppable\"]//div[@class=\"drag_holder\"]"));

                for (int i = 0; i < DragAndDropItems.Count; i++)
                {
                    if (SplittedAnswers[0] == DeleteBrackets(DragAndDropItems[i].GetAttribute("innerHTML")))
                    {
                        ClickMultipleTimes(DragAndDropItems[i], 5);
                        SplittedAnswers.RemoveAt(0);
                        break;
                    }
                }
            }
        }
        private void CheckBoxHelper(string Answer)
        {
            string[] SplittedAnswers = Answer.Split('\n');
            var CheckBoxes = CurrentContentWrap.FindElements(By.XPath(
                $"//section[@id=\"content_wrap_{ContentWrapID}\"]//span[@class=\"is-checkbox-choice-text\"]"));
            foreach (IWebElement CheckBox in CheckBoxes)
            {
                if (SplittedAnswers.Contains(CheckBox.GetAttribute("innerHTML")))
                {
                    ClickMultipleTimes(CheckBox, 4);
                }
            }
        }
        private void AdvancedDragAndDropHelper(string Answer)
        {
            Task.Delay(200).Wait();
            List<string> SplittedAnswers = Answer.Split('\n').ToList();

            while (SplittedAnswers.Count != 0)
            {
                Task.Delay(200).Wait();
                var DragAndDropItems = CurrentContentWrap.FindElements(By.XPath(
                    $"//section[@id=\"content_wrap_{ContentWrapID}\"]//div[@class=\"drop_item_zone ui-droppable has_drag_item\"]"));

                for (int i = 0; i < DragAndDropItems.Count; i++)
                {
                    if (SplittedAnswers[0] == DeleteBrackets(DragAndDropItems[i].GetAttribute("innerHTML")))
                    {
                        ClickMultipleTimes(DragAndDropItems[i], 5);
                        SplittedAnswers.RemoveAt(0);
                        break;
                    }
                }
            }
        }
        private string DeleteBrackets(string html)
        {
            string result = "";
            bool InsideBrackets = false;
            foreach (char i in html)
            {
                if (i == '<')
                    InsideBrackets = true;
                else if (!InsideBrackets)
                    result += i;
                else if (i == '>')
                    InsideBrackets = false;
            }
            return result;
        }
    }
}
