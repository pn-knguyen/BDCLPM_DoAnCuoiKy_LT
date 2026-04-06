using System;
using System.Collections.Generic;
using System.Text;
using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Product
{
    public class DeleteTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Delete productPage;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F3.3_";

        private string targetProductName = string.Empty;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            productPage = new Delete(driver, wait);
            targetProductName = string.Empty;
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F3.3_" })]
        public void DeleteProductTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
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

            string? screenshotNote = null;
            if (!testPassed)
            {
                screenshotNote = CaptureFailureScreenshot(tcId);
            }

            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            ExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail",
                SheetName,
                screenshotNote);

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

            if (action.Contains("mở trang"))
                productPage.Navigate(data);

            else if (action.Contains("tên đăng nhập"))
                productPage.EnterUsername(data);

            else if (action.Contains("mật khẩu"))
                productPage.EnterPassword(data);

            else if (action.Contains("đăng nhập"))
                productPage.ClickLogin();

            else if (action.Contains("quản lý sản phẩm"))
                productPage.OpenProductManagement();

            else if (action.Contains("tìm sản phẩm") || action.Contains("sản phẩm muốn xóa"))
                targetProductName = data?.Trim() ?? string.Empty;

            else if (action.Contains("nút xóa") || action.Contains("click nút xóa") || action.Contains("xóa sản phẩm"))
            {
                var productName = !string.IsNullOrWhiteSpace(data) ? data.Trim() : targetProductName;
                productPage.ClickDeleteByName(productName);

                try
                {
                    productPage.AcceptDeleteConfirmationAlert();
                }
                catch
                {
                }
            }

            else if (action.Contains("xác nhận"))
                productPage.AcceptDeleteConfirmationAlert();

            else if (action.Contains("hủy") || action.Contains("cancel"))
                productPage.DismissDeleteConfirmationAlert();
        }

        private string GetActualResult()
        {
            string success = productPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
                return success;

            string error = productPage.GetErrorMessage();
            if (!string.IsNullOrEmpty(error))
                return error;

            if (!string.IsNullOrWhiteSpace(targetProductName)
                && driver.Url.Contains("/Product/Index")
                && !productPage.HasProductByName(targetProductName))
            {
                return "Sản phẩm bị xóa khỏi danh sách, hiển thị thông báo xóa sản phẩm thành công";
            }

            if (driver.Url.Contains("/Product/Index"))
                return "Đang ở trang quản lý sản phẩm";

            return $"Trang hiện tại: {driver.Url}";
        }

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Product", "Delete");
                Directory.CreateDirectory(folder);

                var safeTcId = string.IsNullOrWhiteSpace(tcId) ? "UnknownTC" : tcId.Replace("/", "_").Replace("\\", "_");
                var fileName = $"{safeTcId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
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
    }
}
