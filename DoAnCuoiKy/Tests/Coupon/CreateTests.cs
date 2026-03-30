using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace DoAnCuoiKy.Tests.Coupon
{
    public class CreateTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private CreateCoupon couponPage; // Sử dụng class CreateCoupon đã tạo

        // Tên sheet trong file Excel, có thể đổi lại cho đúng với file của bạn (ví dụ: "Test Cases Coupon")
        private const string SheetName = "Test Cases AD";

        // Cập nhật Filter dựa theo hình ảnh (F9.2_01)
        private const string TestCaseFilter = "F9.2_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            couponPage = new CreateCoupon(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void CreateCouponTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                TestStep? nextStep = i < steps.Count - 1 ? steps[i + 1] : null;

                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");

                ExecuteStep(step, nextStep);
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

        private void ExecuteStep(TestStep step, TestStep? nextStep)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            // Mapping các action từ file Excel (hình ảnh) sang hàm của CreateCoupon
            if (action.Contains("mở trang") || action.Contains("open"))
                couponPage.Navigate(data);

            else if (action.Contains("tên đăng nhập") || action.Contains("username"))
                couponPage.EnterUsername(data);

            else if (action.Contains("mật khẩu") || action.Contains("password"))
                couponPage.EnterPassword(data);

            else if (action.Contains("nút đăng nhập") || action.Contains("login"))
                couponPage.ClickLogin();

            else if (action.Contains("quản lý coupon"))
                couponPage.OpenCouponManagement();

            else if (action.Contains("tạo coupon mới"))
                couponPage.ClickCreateCoupon();

            else if (action.Contains("mã coupon"))
                couponPage.EnterCode(data);

            else if (action.Contains("số tiền giảm"))
                couponPage.EnterDiscountAmount(data);

            else if (action.Contains("ngày hết hạn"))
                couponPage.EnterExpiryDate(data);

            // Thêm điều kiện click Hủy từ Step 10
            else if (action.Contains("click hủy") || action.Contains("chọn hủy"))
                couponPage.ClickCancel();

            else if (action.Contains("tạo coupon") || action.Contains("lưu"))
                couponPage.Submit();
        }

        private string GetActualResult()
        {
            string alertText = couponPage.GetAlertTextAndAccept();
            if (!string.IsNullOrEmpty(alertText))
            {
                return alertText;
            }

            string currentUrl = driver.Url;

            string success = couponPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
                return success;

            string validation = couponPage.GetValidationErrors();
            if (!string.IsNullOrEmpty(validation))
                return validation;

            // SỬA LẠI ĐOẠN NÀY: Bắt cả trường hợp URL chỉ có /Coupon hoặc kết thúc bằng dấu /
            if (currentUrl.EndsWith("/Coupon") || currentUrl.EndsWith("/Coupon/") || currentUrl.EndsWith("/Coupon/Index"))
            {
                return "Quay về trang danh sách";
            }

            if (currentUrl.Contains("/Coupon/Create"))
                return "Vẫn ở trang tạo coupon";

            return $"Trang hiện tại: {currentUrl}";
        }

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Coupon", "Create");
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