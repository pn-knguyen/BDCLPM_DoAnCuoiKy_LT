using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Register_Customer
{
    public class RegisterPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RegisterPage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement ClickToRegister => wait.Until(d => d.FindElement(By.CssSelector("a[class='bg-primary-600 text-white hover:bg-primary-700 px-4 py-2 rounded-md text-sm font-medium transition-colors']")));
        private IWebElement FirstName => wait.Until(d => d.FindElement(By.CssSelector("#FirstName")));
        private IWebElement LastName => wait.Until(d => d.FindElement(By.CssSelector("#LastName")));
        private IWebElement Email => wait.Until(d => d.FindElement(By.CssSelector("#Email")));
        private IWebElement PhoneNumber => wait.Until(d => d.FindElement(By.CssSelector("#Phone")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.CssSelector("#Password")));
        private IWebElement VerifyPassword => wait.Until(d => d.FindElement(By.CssSelector("#ConfirmPassword")));
    }
}