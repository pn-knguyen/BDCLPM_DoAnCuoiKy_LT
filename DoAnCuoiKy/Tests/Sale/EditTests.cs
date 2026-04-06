using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using DoAnCuoiKy.Pages.Sale;

namespace DoAnCuoiKy.Tests.Sale
{
    public class EditTests
    {
        public Edit editPage;
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F6.2_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            editPage = new Edit(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F6.2_" })]
        public void SaleEditTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);
            string data = string.Empty;
            string obj = string.Empty;
            switch (objective?.ToLower())
            {
                case string s when s.Contains("tên chương trình"): obj = "#discountName"; break;
                case string s when s.Contains("phần trăm giảm"): obj = "#discountPercent"; break;
            }
            foreach (var step in steps)
            {
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                if (step.StepNumber == 8)
                    data = step.TestData ?? "";
                ExecuteStep(step);
            }

            string actualResult = GetActualResult(data, obj);

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
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Sale", "Edit");
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
                editPage.Navigate(data);
            }

            else if (action.Contains("username") || action.Contains("tên đăng nhập"))
            {
                editPage.EnterUsername(data);
            }

            else if (action.Contains("password") || action.Contains("mật"))
            {
                editPage.EnterPassword(data);
            }

            else if (action.Contains("login") || action.Contains("đăng nhập"))
            {
                editPage.ClickLogin();
            }
            else if (action.Contains("trang giảm giá") || action.Contains("sale management"))
            {
                editPage.OpenSaleManagement();
            }
            else if (action.Contains("nút sửa") || action.Contains("chọn sửa") || action.Contains("edit sale"))
            {
                editPage.ClickEditSale(data);
            }
            else if (action.Contains("tên chương trình") || action.Contains("sale name"))
            {
                if (string.IsNullOrEmpty(data))
                    editPage.ClearSaleName();
                else
                    editPage.EnterSaleName(data);
            }
            else if (action.Contains("phần trăm giảm") || action.Contains("discount by percentage"))
            {
                if (string.IsNullOrEmpty(data))
                    editPage.ClearSalePercent();
                else
                    editPage.EnterSalePercent(data);
            }
            else if (action.Contains("lưu") || action.Contains("save sale") || action.Contains("cập nhật"))
            {
                editPage.ClickSubmit();
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

        private string GetActualResult(string data, string obj)
        {
            string error = editPage.GetErrorMessage(obj);

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }
            string success = editPage.GetSuccessMessage(data);
            if (!string.IsNullOrEmpty(success))
            {
                return success;
            }
            return "";
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
