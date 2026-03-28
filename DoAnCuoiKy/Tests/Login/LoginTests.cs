using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using DoAnCuoiKy.Pages.Login;

namespace DoAnCuoiKy.Tests.Login
{
    public class LoginTests
    {
        public LoginPage loginPage;
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            loginPage = new LoginPage(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F1_" })]
        public void LoginTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);

            foreach (var step in steps)
            {
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                ExecuteStep(step);
            }

            string actualResult = GetActualResult();

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Expected: {expectedResult}");
            TestContext.Out.WriteLine($"Actual: {actualResult}");

            bool testPassed = false;

            if (!string.IsNullOrEmpty(expectedResult))
            {
                string expected = NormalizeString(expectedResult);
                string actual = NormalizeString(actualResult);
                testPassed = actual.Contains(expected) || expected.Contains(actual) || actual.Equals(expected);
            }

            if (!testPassed)
            {
                CaptureFailureScreenshot(tcId);
            }

            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            ExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail",
                SheetName);

            Assert.That(testPassed, Is.True);
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            return input
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ")
                .ToLower()
                .Trim()
                .Replace("  ", " ");
        }

        private void CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Login");
                Directory.CreateDirectory(folder);

                var fileName = $"{tcId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(folder, fileName);

                screenshot.SaveAsFile(filePath);
                TestContext.Out.WriteLine($"Screenshot saved: {filePath}");
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"ERROR taking screenshot: {ex.Message}");
            }
        }

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            if (action.Contains("mở") || action.Contains("navigate"))
            {
                loginPage.Navigate(data);
            }

            else if (action.Contains("username") || action.Contains("tên"))
            {
                loginPage.EnterUsername(data);
            }

            else if (action.Contains("password") || action.Contains("mật"))
            {
                loginPage.EnterPassword(data);
            }

            else if (action.Contains("click") || action.Contains("login") || action.Contains("đăng nhập"))
            {
                loginPage.ClickLogin();
            }
        }

        private IWebElement WaitForElement(By locator)
        {
            return wait.Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            });
        }

        private string GetActualResult()
        {
            string currentUrl = driver.Url;

            if (currentUrl.Contains("/Dashboard"))
            {
                return $"Chuyển hướng đến trang admin: {currentUrl}";
            }

            string error = loginPage.GetErrorMessage();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            return $"Still on login page: {currentUrl}";
        }

        private IWebElement? FindElement(params By[] locators)
        {
            foreach (var locator in locators)
            {
                try
                {
                    var element = driver.FindElement(locator);
                    if (element.Displayed)
                        return element;
                }
                catch { }
            }

            return null;
        }
    }
}
