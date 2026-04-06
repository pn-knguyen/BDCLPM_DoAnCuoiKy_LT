using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Tests.Product
{
    public class EditTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Edit productPage;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F3.4_";

        private int colorIndex = 0;

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            productPage = new Edit(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases AD", "F3.4_" })]
        public void EditProductTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

            else if (action.Contains("click nút sửa"))
                productPage.ClickEditProduct();

            else if (action.Contains("tên sản phẩm"))
                productPage.EnterProductName(data);

            else if (action.Contains("danh mục"))
                productPage.SelectCategory(data);

            else if (action.Contains("giảm giá"))
                productPage.SelectDiscount(data);

            else if (action.Contains("nhập giá") || action.Contains("price"))
                productPage.EnterPrice(data);

            else if (action.Contains("số lượng"))
                productPage.EnterStock(data);

            else if (action.Contains("mô tả ngắn"))
                productPage.EnterShortDescription(data);

            else if (action.Contains("mô tả chi tiết"))
                productPage.EnterDetailDescription(data);

            else if (action.Contains("thêm màu"))
            {
                productPage.AddColor();
                var count = productPage.GetColorCount();
                if (count > 0)
                {
                    colorIndex = count - 1;
                }
            }

            else if (action.Contains("chọn màu"))
                productPage.SelectColor(data, colorIndex);

            else if (action.Contains("xóa màu"))
                productPage.RemoveColor();

            else if (action.Contains("chọn hình") || action.Contains("upload"))
            {
                var imagePaths = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (imagePaths.Length == 0)
                {
                    productPage.UploadNewImage(data, colorIndex);
                }
                else
                {
                    foreach (var path in imagePaths)
                    {
                        productPage.UploadNewImage(path.Trim(), colorIndex);
                    }
                }
            }
            else if (action.Contains("delete") || action.Contains("xóa") || action.Contains("click nút x"))
            {
                int count = 1;

                if (!string.IsNullOrWhiteSpace(data) && int.TryParse(data, out int parsed))
                    count = parsed;

                for (int i = 0; i < count; i++)
                {
                    productPage.RemoveImage(0);

                    try
                    {
                        wait.Until(d =>
                        {
                            var buttons = d.FindElements(By.CssSelector(".relative .w-4"));
                            return buttons.Count == 0 || !buttons[0].Displayed || !buttons[0].Enabled;
                        });
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Thread.Sleep(500);
                    }
                }
            }

            else if (action.Contains("lưu"))
                productPage.SubmitEdit();
        }

        private string GetActualResult()
        {
            string currentUrl = driver.Url;

            // lấy thông báo thành công
            string success = productPage.GetSuccessMessage();
            if (!string.IsNullOrEmpty(success))
                return success;

            // lấy lỗi validation
            string validation = productPage.GetValidationErrors();
            if (!string.IsNullOrEmpty(validation))
                return $"Lỗi validation: {validation}";

            // case nghiệp vụ: không cho xóa ảnh cuối cùng -> nút lưu bị disable
            if (currentUrl.Contains("/Product/Edit")
                && productPage.IsSubmitDisabled()
                && productPage.GetRemoveImageButtonCount() <= 1)
            {
                return "Nút cập nhật bị disable";
            }

            // case nghiệp vụ: form không hợp lệ (ví dụ tên sản phẩm trống) -> nút bị disable
            if (currentUrl.Contains("/Product/Edit") && productPage.IsSubmitDisabled())
                return "Nút sản phẩm bị disable";

            // fallback theo url
            if (currentUrl.Contains("/Product/Index"))
                return "Cập nhật sản phẩm thành công";

            if (currentUrl.Contains("/Product/Edit"))
                return "Vẫn ở trang chỉnh sửa sản phẩm (có thể có lỗi)";

            return $"Trang hiện tại: {currentUrl}";
        }

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Product", "Edit");
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