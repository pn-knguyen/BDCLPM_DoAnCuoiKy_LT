using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Product
{
    public class CreateProductTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private CreatePage productPage;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F3.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            productPage = new CreatePage(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F3.1_" })]
        public void CreateProductTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

            if (action.Contains("mở trang") || action.Contains("open"))
                productPage.Navigate(data);

            else if (action.Contains("tên đăng nhập") || action.Contains("username"))
                productPage.EnterUsername(data);

            else if (action.Contains("mật khẩu") || action.Contains("password"))
                productPage.EnterPassword(data);

            else if (action.Contains("đăng nhập") || action.Contains("login"))
                productPage.ClickLogin();

            else if (action.Contains("quản lý sản phẩm"))
                productPage.OpenProductManagement();

            else if (action.Contains("thêm sản phẩm mới"))
                productPage.ClickCreateProduct();

            else if (action.Contains("tên sản phẩm"))
                productPage.EnterProductName(data);

            else if (action.Contains("danh mục"))
                productPage.SelectCategory(data);

            else if (action.Contains("nhập giá") && !action.Contains("giảm"))
                productPage.EnterPrice(data);

            else if (action.Contains("số lượng") || action.Contains("tồn kho"))
                productPage.EnterStock(data);

            else if (action.Contains("giảm giá") || action.Contains("discount"))
                productPage.SelectDiscount(data);

            else if (action.Contains("mô tả ngắn"))
                productPage.EnterShortDescription(data);

            else if (action.Contains("mô tả chi tiết") || action.Contains("mô tả"))
                productPage.EnterDetailDescription(data);

            else if (action.Contains("màu"))
                productPage.SelectColor(data);

            else if (action.Contains("upload") || action.Contains("hình"))
                productPage.UploadImage(data);

            else if (action.Contains("lưu") || action.Contains("submit"))
                productPage.Submit();
        }

        private string GetActualResult()
        {
            string currentUrl = driver.Url;

            string success = productPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
                return success;

            string validation = productPage.GetValidationErrors();
            if (!string.IsNullOrEmpty(validation))
                return $"Lỗi validation: {validation}";

            if (currentUrl.Contains("/Product/Index"))
                return "Sản phẩm được tạo thành công, quay về trang danh sách";

            if (currentUrl.Contains("/Product/Create"))
                return "Vẫn ở trang tạo sản phẩm (có thể có lỗi)";

            return $"Trang hiện tại: {currentUrl}";
        }
    }
}

