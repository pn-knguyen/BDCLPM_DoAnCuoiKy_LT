using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages.Category
{
    public class Create
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly CategoryIndexPage _indexPage;

        public Create(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
            _indexPage = new CategoryIndexPage(driver, wait);
        }

        private IWebElement Wait(By locator)
        {
            return _wait.Until(d => d.FindElement(locator));
        }

        public void Navigate(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public void EnterUsername(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            var username = Wait(By.Id("Username"));
            username.Clear();
            username.SendKeys(data);
        }

        public void EnterPassword(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            var password = Wait(By.Id("Password"));
            password.Clear();
            password.SendKeys(data);
        }

        public void ClickLogin()
        {
            Wait(By.CssSelector("button[type='submit']")).Click();
        }

        public void OpenCategoryManagement()
        {
            Wait(By.LinkText("Quản lý danh mục")).Click();
        }

        public void ClickCreateCategory()
        {
            _indexPage.ClickAddNewCategory();
        }

        public void EnterCategoryId(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            var categoryId = Wait(CategoryIndexLocators.CategoryIdInput);
            categoryId.Clear();
            categoryId.SendKeys(data);
        }

        public void EnterCategoryName(string data)
        {
            var categoryName = Wait(CategoryIndexLocators.CategoryNameInput);
            categoryName.Clear();

            if (!string.IsNullOrWhiteSpace(data))
            {
                categoryName.SendKeys(data);
            }
        }

        public void Submit()
        {
            Wait(CategoryIndexLocators.FormSubmitButton).Click();
        }

        public void Cancel()
        {
            _indexPage.ClickCancelModal();
        }

        public bool IsModalOpen()
        {
            return _indexPage.IsModalOpen();
        }

        public bool IsCategoryTableVisible()
        {
            return _indexPage.IsTableVisible();
        }

        public string GetSuccessMessage()
        {
            try
            {
                var msg = _wait.Until(d =>
                {
                    var successSelectors = new[]
                    {
                        ".sm\\:inline",
                        ".bg-green-100",
                        ".text-green-700"
                    };

                    foreach (var selector in successSelectors)
                    {
                        var found = d.FindElements(By.CssSelector(selector))
                            .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                        if (found != null)
                        {
                            return found;
                        }
                    }

                    return null;
                });

                return msg?.Text ?? string.Empty;
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
                var errors = _driver.FindElements(By.CssSelector(
                    ".text-red-500, .validation-error, .text-danger, span.field-validation-error, .bg-red-100, .text-red-700"));

                return string.Join(", ", errors.Select(e => e.Text).Where(t => !string.IsNullOrWhiteSpace(t)));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
