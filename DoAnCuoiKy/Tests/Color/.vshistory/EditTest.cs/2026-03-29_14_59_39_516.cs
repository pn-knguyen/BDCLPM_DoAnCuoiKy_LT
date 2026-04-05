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
    public class EditTest
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Edit colorPage;
        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F5.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            colorPage = new Edit(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
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
            else if (action.Contains("thêm màu mới"))
                colorPage.ClickEditColor();
            else if (action.Contains("nhập mã màu"))
                colorPage.EnterColor(data);
            else if (action.Contains("thêm màu sắc") || action.Contains("lưu"))
                colorPage.Submit();
        }

        private string GetActualResult(string data)
        {
            string success = colorPage.GetSuccessMessage(data);
            if (!string.IsNullOrEmpty(success))
                return success;
            string validation = colorPage.GetValidationErrors();
            if (!string.IsNullOrEmpty(validation))
                return $"Lỗi validation: {validation}";
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