using System;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
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
        private bool IsFillingAnswers = false;
        private IWebElement CurrentContentWrap;
        private int ContentWrapID;
        public DriverRework(Func<string, MessageBoxResult> ErrorMessagesNotifier)
        {
            PrintErrorMessage = ErrorMessagesNotifier;
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.UnhandledException += new UnhandledExceptionEventHandler(ErrorHandler);

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var chromeOptions = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.None
            };
            try
            {
                driver = new ChromeDriver(driverService, chromeOptions)
                {
                    Url = "https://www.cambridgeone.org/login"
                };

                IsRunning = true;
            }
            catch (DriverServiceNotFoundException)
            {
                PrintErrorMessage("Произошел конфликт с версией браузера Google Chrome. Если у вас последняя версия браузера - сообщите в группу.");
            }
            catch (WebDriverException)
            {
                PrintErrorMessage("Данная программа поддерживает только 100 версию Chrome. Либо нам нужно обновиться, либо вам.");
            }
        }
        private void ErrorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            PrintErrorMessage($"Была обнаружена неизвестная ошибка. Если хотите помочь исправить ее, то напишите в группу.\nТекст ошибки: {e.Message}\nStackTrace: {e.StackTrace}");
            while (true)
            {
                Task.Delay(10000000).Wait();
            }
        }
        public void Close()
        {
            try
            {
                driver.Quit();
                IsRunning = false;
            }
            catch { } // бывает webdriver exception, но на всякий пусть при любой ошибке обрабатывает
        }
        private bool WaitForElement(string xpath, int IterationLimit = 9999) // ожидание Limit * 0.1 секунд
        {
            int Iterations = 0;
            try
            {
                while (CheckForElement(xpath) == false && ++Iterations < IterationLimit)
                {
                    Task.Delay(100).Wait();
                }
                return Iterations < IterationLimit;
            }
            catch
            {
                return false;
            }

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
                        PrintErrorMessage($"Неизвестная ошибка при попытке нажать на \"{xpath}\". Скорее всего вы закрыли окно или ответы не совпадают.");
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
                        //PrintErrorMessage($"Бот не смог найти кнопку");
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
                catch
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
                }
                catch { }
                await Task.Delay(1000);
            }
        }
        public void FillLoginPage()
        {
            if (WaitForElement("//*[@id=\"gigya-loginID-56269462240752180\"]"))
            {
                driver.FindElement(By.Id("gigya-loginID-56269462240752180")).SendKeys(AppConstants.Email);
                driver.FindElement(By.XPath("//input[@value=\"Log in\"]")).Click();
            }
        }
        public void FillAnswersMachine(string[] AnswersArray, int[] TasksTag)
        {
            if (IsFillingAnswers)
                return;
            else
                IsFillingAnswers = true;

            string StartUrl = driver.Url;
            driver.SwitchTo().Frame(driver.FindElement(By.TagName("iframe")));

            // если нет вопросов, то просто нажимаем кнопку Next
            bool IsPresentation = CheckIfPresentation(TasksTag);
            if (IsPresentation)
            {
                SolvePresentation();
                IsFillingAnswers = false;
                return;
            }


            do
            {
                // получаем код задания на экране и его номер по счету
                CurrentContentWrap = GetActiveContentWrap();
                if (CurrentContentWrap == null)
                {
                    continue;
                }
                ContentWrapID = int.Parse(CurrentContentWrap.GetAttribute("id").Split('_').Last());


                // ответов бывает меньше, чем заданий (из-за презентаций)
                try
                {
                    if (AnswersArray.Length <= ContentWrapID)
                    {
                        SolveTaskByTag(null, TasksTag[ContentWrapID]);
                    }
                    else
                    {
                        SolveTaskByTag(AnswersArray[ContentWrapID], TasksTag[ContentWrapID]);
                    }
                }
                catch (NoSuchWindowException) { }


            } while (ContentWrapID != TasksTag.Length - 1 && StartUrl == driver.Url);


            driver.SwitchTo().DefaultContent();
            IsFillingAnswers = false;
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
                Task.Delay(1500).Wait();
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
                else if (CheckForElement("//section[@class=\"content-wrap custom-pool\"]"))
                {
                    return driver.FindElement(
                        By.XPath("//section[@class=\"content-wrap custom-pool\"]"));
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
            int InteractionsLimit = 4;
            int Interactions = 0;
            while (Interactions++ < InteractionsLimit)
            {
                try
                {
                    CurrentContentWrap.FindElement(By.XPath($"//section[contains(@style,\"flex\")]//input")).SendKeys(Answer.Replace('\n', '\t'));
                    return;
                }
                catch (NoSuchElementException)
                {
                    CurrentContentWrap.FindElement(By.XPath("//div[@class=\"input-text has-input\"]//input")).SendKeys(Answer.Replace('\n', '\t'));
                    return;
                }
                catch (ElementNotInteractableException) { }
                Task.Delay(300).Wait();
            }
        }
        private void RadioButtonHelper(string Answer)
        {
            var buttons = CurrentContentWrap.FindElements(By.XPath("//span"));
            foreach (IWebElement button in buttons)
            {
                if (DeleteBrackets(button.GetAttribute("innerHTML")) == Answer)
                {
                    if (ClickMultipleTimes(button, 4))
                        return;
                }
            }
            string PossibleAnswer;
            foreach (IWebElement button in buttons)
            {
                try
                {
                    PossibleAnswer = button.GetAttribute("innerHTML")
                    .Split('>')[1].Split('<')[0];
                    if (PossibleAnswer == Answer)
                    {
                        if (ClickMultipleTimes(button, 4))
                            return;
                    }
                }
                catch { }
            }
            var ButtonsGroup = CurrentContentWrap.FindElements(By.XPath($"//section[@id=\"content_wrap_{ContentWrapID}\"]//div[@class=\"contentblock alignment-vertical\"]"));
            List<string> SplittedAnswers = Answer.Split('\n').ToList();
            foreach (IWebElement ButtonGroup in ButtonsGroup)
            {
                var RadioButtons = ButtonGroup.FindElements(By.XPath($"//section[@id=\"content_wrap_{ContentWrapID}\"]//span[@class=\"is-radiobutton-choice-text\"]"));
                foreach (IWebElement RadioButton in RadioButtons)
                {
                    if (SplittedAnswers.Count != 0 && RadioButton.GetAttribute("innerHTML").Contains(SplittedAnswers[0]))
                    {
                        SplittedAnswers.RemoveAt(0);
                        RadioButton.Click();
                    }
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
                        if (ClickMultipleTimes(ComboBoxItem, 5))
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
                    $"//section[@id=\"content_wrap_{ContentWrapID}\"]//div[contains(@class, \"pool ui-droppable\")]//div[@class=\"drag_holder\"]"));

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
            if (CheckBoxes.Count == 0)
            {
                CheckBoxes = CurrentContentWrap.FindElements(By.XPath(
                    $"//section[@id=\"content_wrap_{ContentWrapID}\"]//span[@class=\"item\"]"));
            }

            foreach (IWebElement CheckBox in CheckBoxes)
            {
                string ForDebug = DeleteBrackets(CheckBox.GetAttribute("innerHTML"));
                if (SplittedAnswers.Contains(ForDebug))
                {
                    try
                    {
                        if (!CheckBox.FindElement(By.XPath("./ancestor::div[contains(@class,\"input-checkbox\")]//input")).Selected)
                            ClickMultipleTimes(CheckBox, 4);
                    }
                    catch
                    {
                        ClickMultipleTimes(CheckBox, 4);
                    }

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
            return result.Replace("&nbsp;", " ");
        }
    }
}
