using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Sale
{
    public class Edit
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private IWebElement Username => wait.Until(d => d.FindElement(By.Id("Username")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.Id("Password")));
        private IWebElement LoginButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));
        private IWebElement SaleName => wait.Until(d => d.FindElement(By.CssSelector("#discountName")));
        private IWebElement SalePercent => wait.Until(d => d.FindElement(By.CssSelector("#discountPercent")));
        private IWebElement SaleSubmit => wait.Until(d => d.FindElement(By.CssSelector("button[class='px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600']")));

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

        public void ClickEditSale(string data)
        {
            var row = Wait(By.XPath($"//tbody/tr[td[normalize-space()='{data}']]/td[last()]/button[1]"));
            row.Click();
        }

        public void EnterSaleName(string name)
        {
            SaleName.Clear();
            SaleName.SendKeys(name);
        }

        public void ClearSaleName()
        {
            SaleName.Clear();
        }

        public void EnterSalePercent(string percent)
        {
            SalePercent.Clear();
            SalePercent.SendKeys(percent);
        }

        public void ClearSalePercent()
        {
            SalePercent.Clear();
        }

        public void ClickSubmit()
        {
            SaleSubmit.Click();
        }

        public string GetSuccessMessage(string data)
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.XPath($"//td[normalize-space()='{data}']"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                return "Sửa chương trình thành công" ?? "";
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
