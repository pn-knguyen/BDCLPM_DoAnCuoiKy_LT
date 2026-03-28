using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages.Category
{
    public class CategoryIndexPage
    {
        private readonly IWebDriver _driver;

        
        private readonly WebDriverWait _wait;


        public CategoryIndexPage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }


        public void Navigate(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }


        public string CurrentUrl => _driver.Url;

        public string GetPageHeading()
        {
            var heading = _wait.Until(d => d.FindElement(CategoryIndexLocators.PageTitle));
            return heading.Text.Trim();
        }

        public bool IsTableVisible()
        {
            return _wait.Until(d => d.FindElement(CategoryIndexLocators.CategoryTable)).Displayed;
        }

        public int GetCategoryRowCount()
        {
            return _driver.FindElements(CategoryIndexLocators.CategoryRows).Count;
        }

        public void ClickAddNewCategory()
        {
            _wait.Until(d => d.FindElement(CategoryIndexLocators.AddNewButton)).Click();
        }

        public bool IsModalOpen()
        {
            var modal = _driver.FindElements(CategoryIndexLocators.CategoryModal).FirstOrDefault();
            if (modal == null)
            {
                return false;
            }

            var cssClass = modal.GetAttribute("class") ?? string.Empty;
            return !cssClass.Contains("hidden");
        }

        public void ClickCancelModal()
        {
            _wait.Until(d => d.FindElement(CategoryIndexLocators.FormCancelButton)).Click();
        }

        public void ClickEditByName(string categoryName)
        {
            var row = string.IsNullOrWhiteSpace(categoryName)
                ? _driver.FindElements(By.XPath("//tbody/tr")).FirstOrDefault()
                : _driver.FindElements(By.XPath($"//tbody/tr[td[contains(normalize-space(), \"{categoryName}\")]]")).FirstOrDefault();

            if (row != null)
            {
                row.FindElement(By.XPath(".//button[contains(normalize-space(), 'Sửa')]"))?.Click();
                return;
            }

            _wait.Until(d => d.FindElements(CategoryIndexLocators.EditButtons).FirstOrDefault())?.Click();
        }

        public void ClickDeleteByName(string categoryName)
        {
            var row = string.IsNullOrWhiteSpace(categoryName)
                ? _driver.FindElements(By.XPath("//tbody/tr")).FirstOrDefault()
                : _driver.FindElements(By.XPath($"//tbody/tr[td[contains(normalize-space(), \"{categoryName}\")]]")).FirstOrDefault();

            if (row != null)
            {
                row.FindElement(By.XPath(".//button[contains(normalize-space(), 'Xóa')]"))?.Click();
                return;
            }

            _wait.Until(d => d.FindElements(CategoryIndexLocators.DeleteButtons).FirstOrDefault())?.Click();
        }

        public bool HasCategoryByName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return _driver.FindElements(CategoryIndexLocators.CategoryRows).Count > 0;
            }

            return _driver.FindElements(By.XPath($"//tbody/tr[td[contains(normalize-space(), \"{categoryName}\")]]")).Count > 0;
        }
    }
}
