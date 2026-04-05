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

        public void EnterUsername(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var username = Wait(By.Id("Username"));
                username.Clear();
                username.SendKeys(data);
            }
        }

        public void EnterPassword(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var password = Wait(By.Id("Password"));
                password.Clear();
                password.SendKeys(data);
            }
        }

        public void ClickLogin()
        {
            Wait(By.CssSelector("button[type='submit']")).Click();
        }

        public void OpenColorManagement()
        {
            Wait(By.LinkText("Quản lý màu sắc")).Click();
        }

        public void ClickCreateColor()
        {
            Wait(By.CssSelector("button[class='bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center']")).Click();
        }

        public void EnterColor(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var colorInput = Wait(By.CssSelector("#colorName"));
                colorInput.Clear();
                colorInput.SendKeys(data);
            }
        }

        public void ClickSave()
        {
            Wait(By.CssSelector("button[class='px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600']")).Click();
        }
    }
}