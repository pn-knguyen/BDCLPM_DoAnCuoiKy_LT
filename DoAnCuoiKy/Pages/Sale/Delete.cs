using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Sale
{
    public class Delete
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private IWebElement Username => wait.Until(d => d.FindElement(By.Id("Username")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.Id("Password")));
        private IWebElement LoginButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));

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

        public void EnterUsername(string username)
        {
            Username.Clear();
            Username.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            Password.Clear();
            Password.SendKeys(password);
        }

        public void ClickLogin()
        {
            LoginButton.Click();
        }

        public void OpenSaleManagement()
        {
            Wait(By.LinkText("Quản lý giảm giá")).Click();
        }

        public void ClickDeleteSale(string data, string objective = "")
        {
            string xpath;
            string obj = objective?.ToLower() ?? "";

            if (obj.Contains("chưa có đơn hàng áp dụng"))
            {
                xpath = $"//tbody/tr[td[normalize-space()='{data}'] and td[4][normalize-space()='0']]/td[last()]/button[2]";
            }
            else if (obj.Contains("có đơn hàng áp dụng"))
            {
                xpath = $"//tbody/tr[td[normalize-space()='{data}'] and td[4][number(normalize-space()) > 0]]/td[last()]/button[2]";
            }
            else
            {
                xpath = $"//tbody/tr[td[normalize-space()='{data}']]/td[last()]/button[2]";
            }

            var row = Wait(By.XPath(xpath));
            row.Click();
        }

        public string AcceptDeleteConfirmationAlert()
        {
            // Bắt alert 1: xác nhận xóa
            var localWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var confirmAlert = localWait.Until(d =>
            {
                try { return d.SwitchTo().Alert(); }
                catch (NoAlertPresentException) { return null; }
            });

            confirmAlert.Accept();

            // Chờ alert 2: thông báo lỗi nếu chương trình đang được sử dụng
            try
            {
                var errorWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                var errorAlert = errorWait.Until(d =>
                {
                    try { return d.SwitchTo().Alert(); }
                    catch (NoAlertPresentException) { return null; }
                });

                string errorMessage = errorAlert.Text ?? "";
                errorAlert.Accept();
                return errorMessage;
            }
            catch
            {
                return "";
            }
        }

        public string DismissDeleteConfirmationAlert()
        {
            var localWait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            var confirmAlert = localWait.Until(d =>
            {
                try { return d.SwitchTo().Alert(); }
                catch (NoAlertPresentException) { return null; }
            });

            confirmAlert.Dismiss();
            return "";
        }

        public string GetProductCountBySaleName(string saleName, string objective = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(saleName)) return "";

                string obj = objective?.ToLower() ?? "";
                string xpath;

                if (obj.Contains("chưa có đơn hàng áp dụng"))
                    xpath = $"//tbody/tr[td[normalize-space()='{saleName}'] and td[4][normalize-space()='0']]/td[4]";
                else if (obj.Contains("có đơn hàng áp dụng"))
                    xpath = $"//tbody/tr[td[normalize-space()='{saleName}'] and td[4][number(normalize-space()) > 0]]/td[4]";
                else
                    xpath = $"//tbody/tr[td[normalize-space()='{saleName}']]/td[4]";

                var cell = driver.FindElement(By.XPath(xpath));
                return cell.Text.Trim();
            }
            catch
            {
                return "";
            }
        }

        public bool HasSaleByName(string saleName, string productCount = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(saleName)) return false;

                string xpath = string.IsNullOrEmpty(productCount)
                    ? $"//td[normalize-space()='{saleName}']"
                    : $"//tbody/tr[td[normalize-space()='{saleName}'] and td[4][normalize-space()='{productCount}']]";

                return driver.FindElements(By.XPath(xpath)).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public string GetSuccessMessage(string data, string productCount = "")
        {
            try
            {
                wait.Until(d =>
                {
                    try
                    {
                        string xpath = string.IsNullOrEmpty(productCount)
                            ? $"//td[normalize-space()='{data}']"
                            : $"//tbody/tr[td[normalize-space()='{data}'] and td[4][normalize-space()='{productCount}']]";

                        var elements = d.FindElements(By.XPath(xpath));
                        return elements.Count == 0;
                    }
                    catch
                    {
                        return false;
                    }
                });

                return "Xóa chương trình thành công";
            }
            catch
            {
                return "";
            }
        }

        public string GetErrorMessage(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";
            try
            {
                var el = driver.FindElement(By.CssSelector(data));
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
