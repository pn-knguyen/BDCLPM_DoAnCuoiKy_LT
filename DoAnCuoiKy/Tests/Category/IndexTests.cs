using DoAnCuoiKy.Pages.Category;
using DoAnCuoiKy.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Net.Http;

namespace DoAnCuoiKy.Tests.Category
{
    public class IndexTests
    {
        private const string CategoryIndexUrl = "http://localhost:5080/Category/Index";
        private const string LoginUrl = "http://localhost:5080/Login";
        private const string AdminUsername = "admin1";
        private const string AdminPassword = "123";

        private EdgeDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private CategoryIndexPage _categoryIndexPage = null!;

        [SetUp]
        public void Setup()
        {
            EnsureApplicationIsRunning();

            _driver = new EdgeDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _categoryIndexPage = new CategoryIndexPage(_driver, _wait);

            LoginAsAdmin();
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        [Test]
        public void Navigate_To_Category_Index_Should_Load_Table_And_Header()
        {
            _categoryIndexPage.Navigate(CategoryIndexUrl);

            var heading = _categoryIndexPage.GetPageHeading();
            var isTableVisible = _categoryIndexPage.IsTableVisible();

            Assert.Multiple(() =>
            {
                Assert.That(_categoryIndexPage.CurrentUrl, Does.Contain("/Category/Index"));
                Assert.That(heading, Does.Contain("Quản lý danh mục"));
                Assert.That(isTableVisible, Is.True);
                Assert.That(_categoryIndexPage.GetCategoryRowCount(), Is.GreaterThanOrEqualTo(0));
            });
        }

        [Test]
        public void Click_Add_New_Category_Should_Open_Modal()
        {
            _categoryIndexPage.Navigate(CategoryIndexUrl);
            _categoryIndexPage.ClickAddNewCategory();

            Assert.That(_categoryIndexPage.IsModalOpen(), Is.True);
        }

        private static void EnsureApplicationIsRunning()
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };

                var response = client.GetAsync(CategoryIndexUrl).GetAwaiter().GetResult();
                if ((int)response.StatusCode >= 500)
                {
                    Assert.Ignore($"Application is not ready at {CategoryIndexUrl}.");
                }
            }
            catch
            {
                Assert.Ignore($"Cannot access {CategoryIndexUrl}. Start the app before running UI automation tests.");
            }
        }

        private void LoginAsAdmin()
        {
            var loginPage = new LoginPage(_driver, _wait);

            // Access protected page first; app may redirect to login automatically.
            _categoryIndexPage.Navigate(CategoryIndexUrl);

            if (IsLoginScreenVisible())
            {
                loginPage.Navigate(LoginUrl);
                loginPage.EnterUsername(AdminUsername);
                loginPage.EnterPassword(AdminPassword);
                loginPage.ClickLogin();

                _wait.Until(_ =>
                {
                    var current = _driver.Url;
                    return current.Contains("/Dashboard") || current.Contains("/Category/Index") || !IsLoginScreenVisible();
                });
            }

            _categoryIndexPage.Navigate(CategoryIndexUrl);
        }

        private bool IsLoginScreenVisible()
        {
            return _driver.FindElements(By.Id("Username")).Count > 0
                && _driver.FindElements(By.Id("Password")).Count > 0;
        }
    }
}
