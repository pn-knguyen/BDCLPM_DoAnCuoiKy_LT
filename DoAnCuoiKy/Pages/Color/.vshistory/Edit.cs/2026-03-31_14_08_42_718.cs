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

        public void ClickEditColor(string targetColorName)
        {
            string dynamicXpath = $"//tr[td[contains(normalize-space(), '{targetColorName}')]]//*[normalize-space(text())='Sửa']";
            IWebElement editButton = Wait(By.XPath(dynamicXpath));
            editButton.Click();
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
            if (!string.IsNullOrEmpty(data))
            {
                var el = IsElementPresent(data);
                result = el ? "Thêm màu mới thành công" : "";
            }
            return result;
        }

        private bool IsElementPresent(string data)
        {
            try
            {
                string dynamicPath = $"//tr[td[normalize-space(text())='{data}']]";
                return wait.Until(d => d.FindElements(By.XPath(dynamicPath)).Count > 0);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public string GetAlertMessage()
        {
            wait.Until(d =>
            {
                try
                {
                    d.SwitchTo().Alert();
                    return true;
                }
                catch (TimeoutException)
                {
                    return false;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            });
            IAlert alert = driver.SwitchTo().Alert();
            string msg = alert.Text ?? "";
            alert.Accept();
            return msg;
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
    }
}