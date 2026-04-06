using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Sale
{
    public class Search
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private IWebElement Username => wait.Until(d => d.FindElement(By.Id("Username")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.Id("Password")));
        private IWebElement LoginButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));
        private IWebElement SearchInput => wait.Until(d => d.FindElement(By.XPath("(//input[@placeholder='Tìm kiếm...'])[1]")));

        public Search(IWebDriver driver, WebDriverWait wait)
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

        public void EnterSearchTerm(string term)
        {
            SearchInput.Clear();
            if (!string.IsNullOrEmpty(term))
                SearchInput.SendKeys(term);
        }

        public void ClearSearch()
        {
            SearchInput.Clear();
        }

        public int GetSearchResultCount()
        {
            try
            {
                return driver.FindElements(By.CssSelector("tbody tr")).Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool HasSaleInResults(string saleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(saleName)) return false;
                return driver.FindElements(By.XPath($"//td[normalize-space()='{saleName}']")).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool HasPartialSaleInResults(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword)) return false;
                return driver.FindElements(By.XPath($"//tbody/tr/td[contains(normalize-space(), '{keyword}')]")).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public string GetEmptyMessage()
        {
            try
            {
                var el = driver.FindElement(By.XPath("//tbody/tr/td[@colspan]"));
                return el.Displayed ? el.Text.Trim() : "";
            }
            catch
            {
                return "";
            }
        }

        public string GetActualSearchResult(string searchTerm)
        {
            try
            {
                // Chờ bảng ổn định sau khi search
                System.Threading.Thread.Sleep(500);

                if (string.IsNullOrEmpty(searchTerm))
                {
                    int count = GetSearchResultCount();
                    return count > 0 ? "Hiển thị toàn bộ chương trình" : "Danh sách trống";
                }

                string emptyMsg = GetEmptyMessage();
                if (!string.IsNullOrEmpty(emptyMsg))
                    return emptyMsg;

                if (HasSaleInResults(searchTerm) || HasPartialSaleInResults(searchTerm))
                    return "Tìm kiếm thành công, hiển thị kết quả phù hợp";

                if (GetSearchResultCount() == 0)
                    return "Không tìm thấy kết quả phù hợp";

                return "Tìm kiếm thành công, hiển thị kết quả phù hợp";
            }
            catch
            {
                return "";
            }
        }
    }
}
