using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Login_Customer
{
    public class LoginPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginPage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement ClickToLogin => wait.Until(d => d.FindElement(By.CssSelector("div[class='flex items-center space-x-2'] a[class='text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors']")));
    }
}