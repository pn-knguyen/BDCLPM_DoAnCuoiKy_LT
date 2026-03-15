using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Product
{
    public class CreateTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Create productPage;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F3.1_";

        private int colorIndex = 0; 
        private bool colorSelected = false;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            productPage = new Create(driver, wait);
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

        private void ExecuteStep(TestStep step, TestStep? nextStep)
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

            else if (action.Contains("nhập giá"))
                productPage.EnterPrice(data);

            else if (action.Contains("số lượng"))
                productPage.EnterStock(data);

            else if (action.Contains("giảm giá"))
                productPage.SelectDiscount(data);

            else if (action.Contains("mô tả ngắn"))
                productPage.EnterShortDescription(data);

            else if (action.Contains("mô tả chi tiết") || action.Contains("mô tả"))
                productPage.EnterDetailDescription(data);

            else if (action.Contains("chọn màu"))
                productPage.SelectColor(data, colorIndex);

            else if (action.Contains("chọn hình") || action.Contains("upload"))
            {
                var imagePaths = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var path in imagePaths)
                {
                    productPage.UploadImage(path.Trim(), colorIndex);
                }

                // chỉ thêm màu nếu bước tiếp theo KHÔNG phải submit
                if (nextStep != null &&
                    !nextStep.StepAction.ToLower().Contains("thêm sản phẩm") &&
                    !nextStep.StepAction.ToLower().Contains("lưu"))
                {
                    productPage.AddColor();
                    colorIndex++;
                }
            }

            else if (action.Contains("thêm sản phẩm") || action.Contains("lưu"))
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

