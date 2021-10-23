using CambridgeOneSolver.Infrastructure;
using CambridgeOneSolver.ViewModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CambridgeOneSolver.Models
{
    internal class Driver
    {
        public static ChromeDriver driver;
        
        public static void Start()
        {
            try
            {
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;

                driver = new ChromeDriver(driverService, new ChromeOptions());
            }
            catch
            {
                ErrorMessages.NoDriver();
                Driver.Quit();
                Application.Current.Shutdown();
            }
            try
            {
                driver.Navigate().GoToUrl("https://www.cambridgeone.org/login");
            }
            catch
            {
                ErrorMessages.NoInternet();
                Driver.Quit();
                Application.Current.Shutdown();
            }
        }
        
        public static void Quit()
        {
            driver.Quit();
        }

        public static string GetDataLink()
        {
            try
            {
                driver.SwitchTo().Frame(driver.FindElementByTagName("iframe"));
            }
            catch
            {
                return "";
            }
            foreach (IWebElement i in driver.FindElementsByTagName("script"))
            {
                if (i.GetAttribute("src").Contains("data.js"))
                {
                    string result = i.GetAttribute("src");
                    driver.SwitchTo().DefaultContent();
                    return result;
                }
            }
            driver.SwitchTo().DefaultContent();
            return "";
        }

        public static async void FillTextBlocks()
        {
            Console.WriteLine("Filling text blocks");
            string content_wrap;
            //display: flex; 
            var StartURL = driver.Url;

            while (StartURL == driver.Url)
            { 
                try
                {
                    driver.SwitchTo().Frame(driver.FindElementByTagName("iframe"));
                    if (ElementCheck($"//section[contains(@style,\"flex\")]//input"))
                    {
                        content_wrap = driver.FindElementByXPath($"//section[contains(@style,\"flex\")]").GetAttribute("id");
                        await WriteAndWait(content_wrap);
                        driver.SwitchTo().DefaultContent();
                    }
                    else
                    {
                        driver.SwitchTo().DefaultContent();
                        await Task.Delay(1000);
                    }
                }
                catch
                {
                    driver.SwitchTo().DefaultContent();
                    await Task.Delay(1000);
                }
            }
            Console.WriteLine("Stopped filling text blocks");
        }
        public static async Task WriteAndWait(string content_wrap)
        {
            int WrapId = int.Parse(Regex.Match(content_wrap, @"\d+").Value);

            Console.WriteLine("Cycle started and is about to write: ", CambridgeWindowViewModel.LatestAnswers[WrapId]);
            driver.FindElementByXPath($"//section[contains(@style,\"flex\")]//input").SendKeys(CambridgeWindowViewModel.LatestAnswers[WrapId].Replace('\n','\t'));
            Console.WriteLine("Written, waiting for a task switch");
            while (ElementCheck($"//section[@id=\"{content_wrap}\" and contains(@style,\"flex\")]"))
                await Task.Delay(2000);
            Console.WriteLine("Switched");
        }

        internal static async Task LoginAsync()
        {
            await WaitElementLoad("//*[@id=\"gigya-loginID-56269462240752180\"]");
            driver.FindElementById("gigya-loginID-56269462240752180").SendKeys(AppConstants.Email);
            driver.FindElementById("gigya-password-56383998600152700").SendKeys(AppConstants.Password);
            driver.FindElementByXPath("//input[@value=\"Log in\"]").Click();
        }

        private static async Task WaitElementLoad(string xpath)
        {
            while (!ElementCheck(xpath))
            {
                await Task.Delay(500);
            }
        }

        private static bool ElementCheck(string xpath)
        {
            if (driver.FindElementsByXPath(xpath).Count == 0)
            {
                return false;
            }
            return true;
        }

        public static async Task ListenLoginAsync()
        {
            string url = "https://www.cambridgeone.org/login";
            while (true)
            {
                if (driver.Url == url) await LoginPageDetectedAsync();
                await Task.Delay(10000);
            }
        }
        
        public static async Task LoginPageDetectedAsync()
        {
            string url = "https://www.cambridgeone.org/login";
            while (driver.Url == url)
            {
                try
                {
                    AppConstants.Email = driver.FindElementByXPath("//input[@id=\"gigya-loginID-56269462240752180\"]").GetAttribute("value");
                    AppConstants.Password = driver.FindElementByXPath("//input[@id=\"gigya-password-56383998600152700\"]").GetAttribute("value");
                    await Task.Delay(400);
                }
                catch
                {
                    await Task.Delay(400);
                }
            }
        }
    }
}
