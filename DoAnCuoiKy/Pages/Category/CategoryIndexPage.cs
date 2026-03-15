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
            var modal = _wait.Until(d => d.FindElement(CategoryIndexLocators.CategoryModal));
            var cssClass = modal.GetAttribute("class") ?? string.Empty;
            return !cssClass.Contains("hidden");
        }
    }
}
