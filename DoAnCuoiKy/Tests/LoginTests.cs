using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy
{
    public class LoginTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
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

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            if (action.Contains("mở") || action.Contains("navigate"))
            {
                driver.Navigate().GoToUrl(data);
            }

            else if (action.Contains("username") || action.Contains("tên"))
            {
                var username = WaitForElement(By.Id("Username"));
                username.Clear();
                username.SendKeys(data);
            }

            else if (action.Contains("password") || action.Contains("mật"))
            {
                var password = WaitForElement(By.Id("Password"));
                password.Clear();
                password.SendKeys(data);
            }

            else if (action.Contains("click") || action.Contains("login") || action.Contains("đăng nhập"))
            {
                var button = WaitForElement(By.CssSelector("button[type='submit']"));
                button.Click();
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

            try
            {
                var error = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".font-medium"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                if (error != null)
                {
                    return error.Text;
                }
            }
            catch
            {
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