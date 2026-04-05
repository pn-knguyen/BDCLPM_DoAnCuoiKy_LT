using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Color
{
    public class Create
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public Create(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement Wait(By locator)
        {
            return wait.Until(d => d.FindElement(locator));
        }

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void ClickAddColor()
        {
            Wait(By.CssSelector("button[class='bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center']")).Click();
        }
    }
}