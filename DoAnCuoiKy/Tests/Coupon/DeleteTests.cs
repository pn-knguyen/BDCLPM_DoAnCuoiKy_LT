using System;
using System.Collections.Generic;
using System.IO;
using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Coupon
{
    public class DeleteTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private DeleteCoupon couponPage; // Chuyển từ Delete (Product) sang DeleteCoupon

        private const string SheetName = "Test Cases AD";
        // Cập nhật filter thành F9.5_ theo đúng Test Case ID trong file Excel
        private const string TestCaseFilter = "F9.5_";

        private string targetCouponCode = string.Empty;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            couponPage = new DeleteCoupon(driver, wait);
            targetCouponCode = string.Empty;
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void DeleteCouponTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

            // Mapping theo các bước từ Step 1 -> Step 8 trong ảnh
            if (action.Contains("mở trang"))
                couponPage.Navigate(data);

            else if (action.Contains("tên đăng nhập"))
                couponPage.EnterUsername(data);

            else if (action.Contains("mật khẩu"))
                couponPage.EnterPassword(data);

            else if (action.Contains("nút đăng nhập") || action.Contains("đăng nhập"))
                couponPage.ClickLogin();

            else if (action.Contains("quản lý coupon"))
                couponPage.OpenCouponManagement();

            else if (action.Contains("chọn coupon muốn xóa"))
                targetCouponCode = data?.Trim() ?? string.Empty;

            else if (action.Contains("click icon delete") || action.Contains("click xóa") || action.Contains("nút xóa"))
            {
                var code = !string.IsNullOrWhiteSpace(data) ? data.Trim() : targetCouponCode;
                couponPage.ClickDeleteByCode(code);
            }

            else if (action.Contains("chọn xác nhận xóa") || action.Contains("xác nhận xóa"))
            {
                try
                {
                    couponPage.ClickConfirmDeleteButton();
                    couponPage.AcceptDeleteConfirmationAlert();
                }
                catch
                {
                    // Catch khi không có form confirm hoặc alert (ví dụ: nút bị disable)
                }
            }

            else if (action.Contains("chọn hủy") || action.Contains("nút hủy"))
            {
                try
                {
                    couponPage.ClickCancel();
                }
                catch
                {
                    // Catch nếu không tìm được nút hủy
                }
            }
        }

        private string GetActualResult()
        {
            string currentUrl = driver.Url;

            // Nếu đang ở trang Delete, bỏ qua warning message (đó là cảnh báo từ form)
            if (currentUrl.Contains("/Coupon/Delete"))
                return "Đang ở trang xác nhận xóa coupon";

            // 1. Kiểm tra xem đã về trang Index/Coupon và mã Coupon đã biến mất khỏi bảng chưa (xóa thành công)
            if (!string.IsNullOrWhiteSpace(targetCouponCode)
                && (currentUrl.EndsWith("/Coupon") || currentUrl.EndsWith("/Coupon/") || currentUrl.Contains("/Coupon/Index"))
                && !couponPage.HasCouponByCode(targetCouponCode))
            {
                return "Hiển thị thông báo \"Xóa coupon thành công!\"";
            }

            // 2. Nếu ở trang Coupon Index, không check warning message (warning là từ các coupon khác)
            if (currentUrl.Contains("/Coupon/Index") || currentUrl.EndsWith("/Coupon"))
                return "Quay lại trang danh sách coupon";

            // 3. Kiểm tra success message (thông báo xanh)
            string success = couponPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
                return success;

            // 4. Bắt các thông báo lỗi hoặc cảnh báo (thông báo đỏ/vàng)
            string warning = couponPage.GetWarningMessage();
            if (!string.IsNullOrEmpty(warning))
                return warning;

            return $"Trang hiện tại: {currentUrl}";
        }

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Coupon", "Delete");
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