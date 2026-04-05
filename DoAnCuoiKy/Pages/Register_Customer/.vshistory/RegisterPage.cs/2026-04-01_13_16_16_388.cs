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
        private IWebElement AgreeTerms => wait.Until(d => d.FindElement(By.CssSelector("#agree-terms")));
        private IWebElement RegisterButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void ClickToRegisterPage()
        {
            ClickToRegister.Click();
        }

        public void EnterFirstName(string firstName)
        {
            FirstName.Clear();
            FirstName.SendKeys(firstName);
        }

        public void EnterLastName(string lastName)
        {
            LastName.Clear();
            LastName.SendKeys(lastName);
        }

        public void EnterEmail(string email)
        {
            Email.Clear();
            Email.SendKeys(email);
        }

        public void EnterPhoneNumber(string phoneNumber)
        {
            PhoneNumber.Clear();
            PhoneNumber.SendKeys(phoneNumber);
        }

        public void EnterPassword(string password)
        {
            Password.Clear();
            Password.SendKeys(password);
        }

        public void EnterVerifyPassword(string verifyPassword)
        {
            VerifyPassword.Clear();
            VerifyPassword.SendKeys(verifyPassword);
        }

        public void ClickAgreeTerms()
        {
            AgreeTerms.Click();
        }

        public void Submit()
        {
            RegisterButton.Click();
        }

        public string GetErrorMessage()
        {
            try
            {
                var errorElement = driver.FindElement(By.CssSelector("div[class='text-red-500 text-sm mt-1']"));
                return errorElement.Text;
            }
            catch
            {
                return "";
            }
        }

        public string GetValidationErrors()
        {
            try
            {
                var el = driver.FindElement(By.CssSelector("#colorName"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                string errorMessage = (string)js.ExecuteScript("return arguments[0].validationMessage;", el);
                return errorMessage ?? "";
            }
            catch
            {
                return "";
            }
        }

        public string GetSuccessMessage()
        {
            var url = driver.Url;
            if (url == "http://localhost:5167/")
            {
                return "Đăng ký thành công";
            }
            else
            {
                return "";
            }
        }
    }
}