using DoAnCuoiKy.Pages.Login;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Category
{
    public abstract class CategoryTestBase
    {
        protected const string SheetName = "Test Cases AD";
        private const string DefaultLoginUrl = "http://localhost:5080/AdminAccount/Login";
        private const string DefaultCategoryPath = "/Category/Index";
        private const string DefaultAdminUsername = "admin1";
        private const string DefaultAdminPassword = "123";

        protected EdgeDriver Driver = null!;
        protected WebDriverWait Wait = null!;
        protected LoginPage LoginPage = null!;
        protected string LastDialogMessage = string.Empty;

        [SetUp]
        public void BaseSetup()
        {
            Driver = new EdgeDriver();
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            LoginPage = new LoginPage(Driver, Wait);
            LastDialogMessage = string.Empty;
            EnsureLoggedIn();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Driver?.Quit();
            Driver?.Dispose();
        }

        protected bool ExecuteLoginStep(string action, string data)
        {
            if (action.Contains("mở trang") || action.Contains("open") || action.Contains("navigate"))
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    Driver.Navigate().GoToUrl(data);
                }

                return true;
            }

            if (action.Contains("tên đăng nhập") || action.Contains("username"))
            {
                LoginPage.EnterUsername(data);
                return true;
            }

            if (action.Contains("mật khẩu") || action.Contains("password"))
            {
                LoginPage.EnterPassword(data);
                return true;
            }

            if (action.Contains("đăng nhập") || action.Contains("login") || action.Contains("click login"))
            {
                LoginPage.ClickLogin();
                return true;
            }

            return false;
        }

        private void EnsureLoggedIn()
        {
            string loginUrl = Environment.GetEnvironmentVariable("BDCLPM_LOGIN_URL") ?? DefaultLoginUrl;
            string categoryUrl = ResolveCategoryUrl(loginUrl);
            string username = Environment.GetEnvironmentVariable("BDCLPM_ADMIN_USERNAME") ?? DefaultAdminUsername;
            string password = Environment.GetEnvironmentVariable("BDCLPM_ADMIN_PASSWORD") ?? DefaultAdminPassword;

            Driver.Navigate().GoToUrl(categoryUrl);

            if (!IsLoginScreenVisible())
            {
                return;
            }

            LoginPage.Navigate(loginUrl);
            LoginPage.EnterUsername(username);
            LoginPage.EnterPassword(password);
            LoginPage.ClickLogin();

            Wait.Until(_ =>
            {
                var currentUrl = Driver.Url;
                return currentUrl.Contains("/Dashboard")
                    || currentUrl.Contains("/Category/Index")
                    || !IsLoginScreenVisible();
            });

            Driver.Navigate().GoToUrl(categoryUrl);
        }

        protected string GetCategoryIndexUrl()
        {
            string loginUrl = Environment.GetEnvironmentVariable("BDCLPM_LOGIN_URL") ?? DefaultLoginUrl;
            return ResolveCategoryUrl(loginUrl);
        }

        protected string GetCurrentUrl()
        {
            return Driver.Url;
        }

        protected string AcceptAlertIfPresent(int timeoutSeconds = 2)
        {
            try
            {
                var alert = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
                    .Until(d => d.SwitchTo().Alert());
                string text = alert.Text ?? string.Empty;
                alert.Accept();
                LastDialogMessage = text;
                return text;
            }
            catch
            {
                return string.Empty;
            }
        }

        protected string DismissAlertIfPresent(int timeoutSeconds = 2)
        {
            try
            {
                var alert = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
                    .Until(d => d.SwitchTo().Alert());
                string text = alert.Text ?? string.Empty;
                alert.Dismiss();
                LastDialogMessage = text;
                return text;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ResolveCategoryUrl(string loginUrl)
        {
            var envCategoryUrl = Environment.GetEnvironmentVariable("BDCLPM_CATEGORY_URL");
            if (!string.IsNullOrWhiteSpace(envCategoryUrl))
            {
                return envCategoryUrl;
            }

            if (Uri.TryCreate(loginUrl, UriKind.Absolute, out var loginUri))
            {
                return $"{loginUri.Scheme}://{loginUri.Authority}{DefaultCategoryPath}";
            }

            return $"http://localhost:5080{DefaultCategoryPath}";
        }

        private bool IsLoginScreenVisible()
        {
            return Driver.FindElements(By.Id("Username")).Count > 0
                && Driver.FindElements(By.Id("Password")).Count > 0;
        }

        protected string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ")
                .ToLower()
                .Trim()
                .Replace("  ", " ");
        }

        protected bool IsExpectedMatched(string? expectedResult, string actualResult)
        {
            if (string.IsNullOrWhiteSpace(expectedResult))
            {
                return false;
            }

            string expected = NormalizeString(expectedResult);
            string actual = NormalizeString(actualResult);
            return actual.Contains(expected) || expected.Contains(actual) || actual.Equals(expected);
        }
    }
}
