using CambridgeOneSolver.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
                await Task.Delay(100);
            }
        }

        private static bool ElementCheck(string xpath)
        {
            try
            {
                driver.FindElementByXPath(xpath);
                return true;
            }
            catch
            {
                return false;
            }
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
