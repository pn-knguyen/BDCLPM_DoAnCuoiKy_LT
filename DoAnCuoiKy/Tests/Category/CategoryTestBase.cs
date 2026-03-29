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

        protected string CaptureFailureScreenshot(string tcId)
        {
            try
            {
                if (Driver is not ITakesScreenshot screenshotDriver)
                {
                    return "Không thể chụp screenshot: driver không hỗ trợ.";
                }

                string safeTcId = string.IsNullOrWhiteSpace(tcId) ? "unknown" : tcId.Replace(':', '_').Replace('/', '_').Replace('\\', '_');
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string screenshotDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TestResults", "Screenshots"));

                Directory.CreateDirectory(screenshotDir);

                string screenshotPath = Path.Combine(screenshotDir, $"{safeTcId}_{timestamp}.png");
                var screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);

                return screenshotPath;
            }
            catch (Exception ex)
            {
                return $"Không thể chụp screenshot: {ex.Message}";
            }
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

        protected string ResolveDynamicTestData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            string result = input;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (result.Contains("{AUTO_UNIQUE}", StringComparison.OrdinalIgnoreCase))
            {
                string unique = $"ZCAT_{timestamp}_{Guid.NewGuid():N}"[..26];
                result = result.Replace("{AUTO_UNIQUE}", unique, StringComparison.OrdinalIgnoreCase);
            }

            if (result.Contains("{AUTO_UNIQUE_SPECIAL}", StringComparison.OrdinalIgnoreCase))
            {
                string uniqueSpecial = $"Z@CAT_{timestamp}_{Guid.NewGuid():N}"[..28];
                result = result.Replace("{AUTO_UNIQUE_SPECIAL}", uniqueSpecial, StringComparison.OrdinalIgnoreCase);
            }

            if (result.Contains("{AUTO_UNIQUE_LONG}", StringComparison.OrdinalIgnoreCase))
            {
                string longValue = $"ZCAT_LONG_{timestamp}_{Guid.NewGuid():N}";
                if (longValue.Length < 120)
                {
                    longValue = longValue + new string('X', 120 - longValue.Length);
                }

                result = result.Replace("{AUTO_UNIQUE_LONG}", longValue, StringComparison.OrdinalIgnoreCase);
            }

            return result;
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
