﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CambridgeOneSolver.Models
{
    internal class Driver
    {
        public static ChromeDriver driver;
        public static void Start()
        {
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            driver = new ChromeDriver(driverService, new ChromeOptions());
            try
            {
                driver.Navigate().GoToUrl("https://www.cambridgeone.org/login");
            }
            catch
            {
                ErrorMessages.NoInternet();
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
    }
}