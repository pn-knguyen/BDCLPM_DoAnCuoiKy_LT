using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Color
{
    public class EditColor
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public EditColor(IWebDriver driver, WebDriverWait wait)
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

        // Public wrapper to check whether a color value exists in the table/list.
        public bool IsColorPresent(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            return IsElementPresent(data);
        }

        // Perform edit for a specific color and verify the new value appears in the list.
        // Returns true when the updated color is found after submit; false otherwise.
        public bool EditColorAndVerify(string targetColorName, string newColorValue)
        {
            if (string.IsNullOrEmpty(targetColorName) || string.IsNullOrEmpty(newColorValue))
                return false;

            // Open edit form for the target color
            ClickEditColor(targetColorName);

            // Enter new color value and submit
            EnterColor(newColorValue);
            Submit();

            // Verify the new color value is present in the list
            return IsElementPresent(newColorValue);
        }

        public string GetAlertMessage()
        {
            try
            {
                // Wait until an alert is present and return it. If no alert within timeout, return empty string.
                IAlert alert = wait.Until(d =>
                {
                    try
                    {
                        return d.SwitchTo().Alert();
                    }
                    catch (NoAlertPresentException)
                    {
                        return null;
                    }
                });

                if (alert == null)
                    return string.Empty;

                string msg = alert.Text ?? "";
                alert.Accept();
                return msg;
            }
            catch (WebDriverTimeoutException)
            {
                // No alert appeared within the wait timeout
                return string.Empty;
            }
            catch
            {
                return string.Empty;
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
    }
}