using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using DoAnCuoiKy.Pages.Sale;

namespace DoAnCuoiKy.Tests.Sale
{
    public class SearchTests
    {
        public Search searchPage;
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F6.4_";

        private string searchTerm = string.Empty;
        private bool searchInputAccepted = false;
        private string executeError = string.Empty;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            searchPage = new Search(driver, wait);
            searchTerm = string.Empty;
            searchInputAccepted = false;
            executeError = string.Empty;
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F6.4_" })]
        public void SaleSearchTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

            string? screenshotPath = null;
            if (!testPassed)
            {
                screenshotPath = CaptureFailureScreenshot(tcId);
            }

            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            ExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail",
                SheetName,
                screenshotPath);

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

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Sale", "Search");
                Directory.CreateDirectory(folder);

                var fileName = $"{tcId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine(folder, fileName);

                screenshot.SaveAsFile(filePath);
                TestContext.Out.WriteLine($"Screenshot saved: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"ERROR taking screenshot: {ex.Message}");
                return null;
            }
        }

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            if (action.Contains("mở trang đăng nhập") || action.Contains("navigate"))
            {
                searchPage.Navigate(data);
            }
            else if (action.Contains("username") || action.Contains("tên đăng nhập"))
            {
                searchPage.EnterUsername(data);
            }
            else if (action.Contains("password") || action.Contains("mật"))
            {
                searchPage.EnterPassword(data);
            }
            else if (action.Contains("login") || action.Contains("đăng nhập"))
            {
                searchPage.ClickLogin();
            }
            else if (action.Contains("trang giảm giá") || action.Contains("sale management"))
            {
                searchPage.OpenSaleManagement();
            }
            else if (action.Contains("tìm kiếm") || action.Contains("search") || action.Contains("nhập từ khóa"))
            {
                searchTerm = ParseSearchTerm(data);
                try
                {
                    if (string.IsNullOrEmpty(searchTerm))
                        searchPage.ClearSearch();
                    else
                        searchPage.EnterSearchTerm(searchTerm);
                    searchInputAccepted = true;
                }
                catch (WebDriverTimeoutException)
                {
                    executeError = "Không tìm thấy ô tìm kiếm trên trang";
                }
            }
            else if (action.Contains("xóa từ khóa") || action.Contains("clear search"))
            {
                searchTerm = string.Empty;
                try
                {
                    searchPage.ClearSearch();
                    searchInputAccepted = true;
                }
                catch (WebDriverTimeoutException)
                {
                    executeError = "Không tìm thấy ô tìm kiếm trên trang";
                }
            }
        }

        private string ParseSearchTerm(string data)
        {
            if (string.IsNullOrEmpty(data)) return "";

            int firstQuote = data.IndexOf('"');
            if (firstQuote >= 0)
            {
                int lastQuote = data.LastIndexOf('"');
                if (lastQuote > firstQuote)
                    return data.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
            }

            if (data.Contains(':'))
                return data.Substring(data.IndexOf(':') + 1).Trim();

            return data;
        }

        private string GetActualResult()
        {
            if (!string.IsNullOrEmpty(executeError))
                return executeError;

            if (searchInputAccepted)
            {
                string searchResult = searchPage.GetActualSearchResult(searchTerm);
                if (!string.IsNullOrEmpty(searchResult))
                    return searchResult;
                return "Ô tìm kiếm chấp nhận input";
            }

            return searchPage.GetActualSearchResult(searchTerm);
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
