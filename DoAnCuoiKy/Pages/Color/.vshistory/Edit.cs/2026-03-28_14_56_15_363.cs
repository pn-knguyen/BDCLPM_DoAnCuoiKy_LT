using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Color
{
    public class Edit
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public Edit(IWebDriver driver, WebDriverWait wait)
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

        public void ClickEditColor()
        {
            Wait(By.LinkText("Sửa")).Click();
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

        public void Submit()
        {
            Wait(By.CssSelector("button[class='px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600']")).Click();
        }

        public bool IsSubmitDisabled()
        {
            try
            {
                var submit = Wait(By.Id("button[class='px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600']"));
                var disabledAttr = submit.GetAttribute("disabled");
                var ariaDisabled = submit.GetAttribute("aria-disabled");
                var classes = submit.GetAttribute("class") ?? "";

                return !string.IsNullOrEmpty(disabledAttr)
                       || string.Equals(ariaDisabled, "true", StringComparison.OrdinalIgnoreCase)
                       || classes.Contains("disabled", StringComparison.OrdinalIgnoreCase)
                       || !submit.Enabled;
            }
            catch
            {
                return false;
            }
        }

        public string GetSuccessMessage(string data)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(data))
            {
                try
                {
                    var msg = wait.Until(d =>
                    {
                        try
                        {
                            var el = d.FindElement(By.LinkText(data));
                            return el.Displayed ? el : null;
                        }
                        catch
                        {
                            return null;
                        }
                    });

                    result = msg?.Text ?? "";
                }
                catch
                {
                    result = "";
                }
            }
            return result;
        }

        public string GetValidationErrors()
        {
            try
            {
                var errors = driver.FindElements(By.CssSelector(".text-red-500, .validation-error, .text-danger, span.field-validation-error, .bg-red-100.border.border-red-400.text-red-700.px-4.py-3.rounded.relative.mb-4, .h2"));

                var result = string.Join(", ", errors.Select(e => e.Text).Where(t => !string.IsNullOrWhiteSpace(t)));

                return result;
            }
            catch
            {
                return "";
            }
        }
    }
}