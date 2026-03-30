using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages
{
    public class Delete
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public Delete(IWebDriver driver, WebDriverWait wait)
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

        public void OpenProductManagement()
        {
            Wait(By.LinkText("Quản lý sản phẩm")).Click();
        }

        public void ClickDeleteByName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return;

            string safeProductName = productName.Replace("'", "\"");
            string xpath = $"//h3[contains(normalize-space(), '{safeProductName}')]/ancestor::div[contains(@class, 'bg-white')]//form[contains(@action, '/Product/Delete/')]//button";

            var deleteBtn = wait.Until(d => d.FindElement(By.XPath(xpath)));
            deleteBtn.Click();
        }

        public void AcceptDeleteConfirmationAlert()
        {
            var alert = wait.Until(d =>
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

            alert.Accept();
        }

        public void DismissDeleteConfirmationAlert()
        {
            var alert = wait.Until(d =>
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

            alert.Dismiss();
        }

        public bool HasProductByName(string productName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productName)) return false;

                string safeProductName = productName.Replace("'", "\"");
                string xpath = $"//h3[contains(normalize-space(), '{safeProductName}')]";
                return driver.FindElements(By.XPath(xpath)).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public string GetSuccessMessage()
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".bg-green-100 .sm\\:inline"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                return msg?.Text ?? "";
            }
            catch
            {
                return "";
            }
        }

        public string GetErrorMessage()
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".bg-red-100 .sm\\:inline, .bg-red-100.border.border-red-400.text-red-700.px-4.py-3.rounded.relative.mb-4, .h2"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                return msg?.Text ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}