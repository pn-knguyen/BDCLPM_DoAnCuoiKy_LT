using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Color;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Tests.Color
{
    public class DeleteTest
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Delete colorPage;
        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F5.3_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            colorPage = new Delete(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F5.3_" })]
        public void DeleteColorTest(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();
            ExcelDataProvider.ClearOldResults(steps, SheetName);
            string data = string.Empty;
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                TestStep? nextStep = i < steps.Count - 1 ? steps[i + 1] : null;
                if (step.StepNumber == 6)
                    data = step.TestData ?? "";
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");

                ExecuteStep(step, nextStep);
            }
            string actualResult = GetActualResult(data);

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

        private void ExecuteStep(TestStep step, TestStep? nextStep)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";
            if (action.Contains("mở trang") || action.Contains("open"))
                colorPage.Navigate(data);
            else if (action.Contains("tên đăng nhập") || action.Contains("username"))
                colorPage.EnterUsername(data);
            else if (action.Contains("mật khẩu") || action.Contains("password"))
                colorPage.EnterPassword(data);
            else if (action.Contains("đăng nhập") || action.Contains("login"))
                colorPage.ClickLogin();
            else if (action.Contains("quản lý màu sắc"))
                colorPage.OpenColorManagement();
            else if (action.Contains("màu cần xóa"))
                colorPage.ClickDeleteColor(data);
            else if (action.Contains("xóa") || action.Contains("delete"))
                colorPage.Submit();
        }

        private string GetActualResult(string data)
        {
            // First, check if there is any alert message (e.g., deletion failed)
            string alertMessage = colorPage.GetValidationErrors();
            if (!string.IsNullOrEmpty(alertMessage))
            {
                return $"Hiển thị thông báo:{alertMessage}";
            }

            // If the color is no longer present after deletion, treat as successful delete
            if (!string.IsNullOrEmpty(data) && !colorPage.IsColorPresent(data))
                return "Xóa màu thành công";

            string success = colorPage.GetSuccessMessage(data);
            if (!string.IsNullOrEmpty(success))
                return success;

            // no alert, no explicit success, return empty
            return "";
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
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Color", "Create");
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