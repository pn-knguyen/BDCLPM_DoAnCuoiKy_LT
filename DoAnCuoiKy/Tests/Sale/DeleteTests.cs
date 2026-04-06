using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using DoAnCuoiKy.Pages.Sale;

namespace DoAnCuoiKy.Tests.Sale
{
    public class DeleteTests
    {
        public Delete deletePage;
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F6.3_";

        private string targetSaleName = string.Empty;
        private string targetProductCount = string.Empty;
        private string currentObjective = string.Empty;
        private string alertMessage = string.Empty;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            deletePage = new Delete(driver, wait);
            targetSaleName = string.Empty;
            targetProductCount = string.Empty;
            currentObjective = string.Empty;
            alertMessage = string.Empty;
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F6.3_" })]
        public void SaleDeleteTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);
            currentObjective = objective ?? string.Empty;

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
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Sale", "Delete");
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
                deletePage.Navigate(data);
            }

            else if (action.Contains("username") || action.Contains("tên đăng nhập"))
            {
                deletePage.EnterUsername(data);
            }

            else if (action.Contains("password") || action.Contains("mật"))
            {
                deletePage.EnterPassword(data);
            }

            else if (action.Contains("login") || action.Contains("đăng nhập"))
            {
                deletePage.ClickLogin();
            }
            else if (action.Contains("trang giảm giá") || action.Contains("sale management"))
            {
                deletePage.OpenSaleManagement();
            }
            else if (action.Contains("chương trình cần xóa") || action.Contains("tìm chương trình"))
            {
                targetSaleName = data?.Trim() ?? string.Empty;
            }
            else if (action.Contains("click xóa") || action.Contains("click nút xóa") || action.Contains("xóa chương trình") || action.Contains("delete sale"))
            {
                var saleName = !string.IsNullOrWhiteSpace(data) ? data.Trim() : targetSaleName;
                targetProductCount = deletePage.GetProductCountBySaleName(saleName, currentObjective);
                deletePage.ClickDeleteSale(saleName, currentObjective);

                try
                {
                    alertMessage = deletePage.AcceptDeleteConfirmationAlert();
                }
                catch
                {
                }
            }
            else if (action.Contains("xác nhận"))
            {
                alertMessage = deletePage.AcceptDeleteConfirmationAlert();
            }
            else if (action.Contains("hủy") || action.Contains("cancel"))
            {
                alertMessage = deletePage.DismissDeleteConfirmationAlert();
            }
        }

        private string GetActualResult()
        {
            // Ưu tiên nội dung alert nếu có
            if (!string.IsNullOrEmpty(alertMessage))
            {
                return alertMessage;
            }

            // Chờ element biến mất khỏi danh sách - xác nhận xóa thành công
            string success = deletePage.GetSuccessMessage(targetSaleName, targetProductCount);
            if (!string.IsNullOrEmpty(success))
            {
                return success;
            }

            // Element vẫn còn trong danh sách - hủy xóa hoặc xóa thất bại
            if (!string.IsNullOrWhiteSpace(targetSaleName) && deletePage.HasSaleByName(targetSaleName, targetProductCount))
            {
                return "Chương trình vẫn còn trong danh sách (hủy xóa thành công)";
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
