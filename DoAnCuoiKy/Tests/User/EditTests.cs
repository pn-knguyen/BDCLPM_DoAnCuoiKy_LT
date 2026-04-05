using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Users;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace DoAnCuoiKy.Tests.Users
{
    public class EditTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Edit userEditPage;

        private const string SheetName = "Test Cases CUS";
        private const string TestCaseFilter = "F1.5_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            userEditPage = new Edit(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void EditUserTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

        // Định nghĩa các bước Action tương ứng với cột Step Action trên Excel
        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            if (action.Contains("mở trang đăng nhập"))
                userEditPage.Navigate(data);

            else if (action.Contains("nhập email"))
                userEditPage.EnterEmail(data);

            else if (action.Contains("nhập mật khẩu"))
                userEditPage.EnterPassword(data);

            else if (action.Contains("click đăng nhập"))
                userEditPage.ClickLogin();

            else if (action.Contains("vào trang thông tin cá nhân") || action.Contains("truy cập trang thông tin cá nhân"))
                userEditPage.OpenProfile();

            else if (action.Contains("nhập họ tên mới"))
                userEditPage.EnterName(data);

            else if (action.Contains("nhập số điện thoại"))
                userEditPage.EnterPhone(data);

            else if (action.Contains("nhập năm sinh"))
                userEditPage.EnterBirthYear(data);

            else if (action.Contains("click cập nhật thông tin"))
                userEditPage.ClickUpdate();
        }

        private string GetActualResult()
        {
            // 1. Ưu tiên 1: Kiểm tra lỗi tooltip HTML5 (bị chặn bởi trình duyệt, VD: min="1900")
            string birthYearHtml5Error = userEditPage.GetHtml5ValidationMessage("BirthYear");
            if (!string.IsNullOrEmpty(birthYearHtml5Error))
            {
                // Trả về đúng thông báo của tooltip
                return birthYearHtml5Error;
            }

            // 2. Ưu tiên 2: Kiểm tra lỗi Validation từ Server (chữ đỏ của thẻ span)
            string birthYearServerError = userEditPage.GetSpanValidationError("BirthYear");
            if (!string.IsNullOrEmpty(birthYearServerError))
            {
                // Trả về câu lỗi (VD: "Độ tuổi phải từ 18 đến 80!")
                return birthYearServerError;
            }

            // Tương tự cho Số điện thoại nếu bạn muốn test (F1.5_06)
            string phoneHtml5Error = userEditPage.GetHtml5ValidationMessage("Phone");
            if (!string.IsNullOrEmpty(phoneHtml5Error)) return phoneHtml5Error;

            string phoneServerError = userEditPage.GetSpanValidationError("Phone");
            if (!string.IsNullOrEmpty(phoneServerError)) return phoneServerError;

            // 3. Ưu tiên 3: Nếu không có lỗi gì, kiểm tra thông báo màu xanh
            string success = userEditPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
            {
                // MẸO: Bỏ qua thông báo "Đăng nhập" cũ bị kẹt lại trên màn hình
                if (success.Contains("Đăng nhập"))
                {
                    return "Không có thông báo lỗi, nhưng form không submit được (bị kẹt ở frontend)";
                }
                return success;
            }

            // 4. Nếu không rơi vào các trường hợp trên
            return $"Trang hiện tại: {driver.Url}";
        }

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Users", "Edit");
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