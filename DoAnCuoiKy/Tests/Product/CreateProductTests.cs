using DoAnCuoiKy.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy
{
    public class CreateProductTests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;

        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F3.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
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
                string expected = expectedResult.ToLower().Trim();
                string actual = actualResult.ToLower().Trim();

                testPassed = actual.Contains(expected) || actual.Equals(expected);
            }

            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            ExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail",
                SheetName);

            Assert.That(testPassed, Is.True);
        }

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            // Bước 1: Mở trang đăng nhập
            if (action.Contains("mở trang") || action.Contains("open"))
            {
                driver.Navigate().GoToUrl(data);
                Thread.Sleep(1000);
            }

            // Bước 2: Nhập tên đăng nhập
            else if (action.Contains("tên đăng nhập") || action.Contains("username"))
            {
                var username = WaitForElement(By.Id("Username"));
                username.Clear();
                username.SendKeys(data);
            }

            // Bước 3: Nhập mật khẩu
            else if (action.Contains("mật khẩu") || action.Contains("password"))
            {
                var password = WaitForElement(By.Id("Password"));
                password.Clear();
                password.SendKeys(data);
            }

            // Bước 4: Click nút đăng nhập
            else if (action.Contains("đăng nhập") || action.Contains("login"))
            {
                var loginBtn = WaitForElement(By.CssSelector("button[type='submit']"));
                loginBtn.Click();
                Thread.Sleep(1500);
            }

            // Bước 5: Chọn quản lý sản phẩm
            else if (action.Contains("quản lý sản phẩm"))
            {
                var productMenu = WaitForElement(By.LinkText("Quản lý sản phẩm"));
                productMenu.Click();
                Thread.Sleep(800);
            }

            // Bước 6: Click nút thêm sản phẩm mới
            else if (action.Contains("thêm sản phẩm mới"))
            {
                var createBtn = WaitForElement(By.LinkText("Thêm sản phẩm mới"));
                createBtn.Click();
                Thread.Sleep(800);
            }

            // Bước 7: Nhập tên sản phẩm
            else if (action.Contains("tên sản phẩm") || action.Contains("product name"))
            {
                var productName = WaitForElement(By.Id("ProductName"));
                productName.Clear();
                productName.SendKeys(data);
            }

            // Bước 8: Chọn danh mục
            else if (action.Contains("danh mục") || action.Contains("category"))
            {
                var category = new SelectElement(WaitForElement(By.Id("CategoryId")));
                category.SelectByText(data);
            }

            // Bước 9: Nhập giá
            else if ((action.Contains("nhập giá") || action.Contains("price")) && !action.Contains("giảm"))
            {
                var price = WaitForElement(By.Id("Price"));
                price.Clear();
                price.SendKeys(data);
                Thread.Sleep(300); // Đợi giá trị được set
            }

            // Bước 10: Nhập số lượng tồn kho
            else if (action.Contains("số lượng") || action.Contains("tồn kho") || action.Contains("stock"))
            {
                var stock = WaitForElement(By.Id("Stock"));
                stock.Clear();
                stock.SendKeys(data);
            }

            // Bước 11: Chọn chương trình giảm giá / khuyến mãi (Discount)
            else if (action.Contains("giảm giá") || action.Contains("khuyến mãi") || action.Contains("discount"))
            {
                // Chỉ chọn nếu có data
                if (!string.IsNullOrWhiteSpace(data))
                {
                    var discount = new SelectElement(WaitForElement(By.Id("DiscountId")));
                    discount.SelectByText(data);
                }
            }

            // Bước 12: Nhập mô tả ngắn
            else if (action.Contains("mô tả ngắn") || action.Contains("short description"))
            {
                var shortDesc = WaitForElement(By.Id("ShortDescription"));
                shortDesc.Clear();
                shortDesc.SendKeys(data);
            }

            // Bước 13: Nhập mô tả chi tiết
            else if (action.Contains("mô tả chi tiết") || action.Contains("detail description") || action.Contains("mô tả"))
            {
                var detailDesc = WaitForElement(By.Id("DetailDescription"));
                detailDesc.Clear();
                detailDesc.SendKeys(data);
            }

            // Bước 14: Chọn màu sắc
            else if (action.Contains("màu") || action.Contains("color"))
            {
                var color = new SelectElement(WaitForElement(By.Name("Colors")));
                color.SelectByText(data);
                Thread.Sleep(300);
            }

            // Bước 15: Upload/Chọn hình ảnh
            else if (action.Contains("upload") || action.Contains("hình ảnh") || action.Contains("chọn hình"))
            {
                if (!string.IsNullOrWhiteSpace(data) && File.Exists(data))
                {
                    var image = WaitForElement(By.Name("Images"));
                    image.SendKeys(data);
                    Thread.Sleep(500); // Đợi upload
                }
            }

            // Bước 16: Click lưu / Submit
            else if (action.Contains("click lưu") || action.Contains("submit") || action.Contains("lưu"))
            {
                var submitBtn = WaitForElement(By.Id("submitButton"));
                submitBtn.Click();
                Thread.Sleep(1500);
            }
        }

        private IWebElement WaitForElement(By locator)
        {
            return wait.Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            });
        }

        private string GetActualResult()
        {
            string currentUrl = driver.Url;

            // Kiểm tra thông báo thành công (theo expected result: "Thêm sản phẩm thành công")
            try
            {
                var successMessage = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".sm\\:inline"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                if (successMessage != null && !string.IsNullOrWhiteSpace(successMessage.Text))
                {
                    return successMessage.Text;
                }
            }
            catch
            {
            }

            // Kiểm tra validation errors
            try
            {
                var validationErrors = driver.FindElements(By.CssSelector(".text-red-500, .validation-error, .text-danger, span.field-validation-error"));
                if (validationErrors.Count > 0)
                {
                    var errors = string.Join(", ", validationErrors.Select(e => e.Text).Where(t => !string.IsNullOrWhiteSpace(t)));
                    if (!string.IsNullOrEmpty(errors))
                    {
                        return $"Lỗi validation: {errors}";
                    }
                }
            }
            catch
            {
            }

            // Kiểm tra redirect về Product/Index (thành công)
            if (currentUrl.Contains("/Product/Index"))
            {
                return "Sản phẩm được tạo thành công, quay về trang danh sách";
            }

            // Kiểm tra vẫn ở trang Create (có lỗi)
            if (currentUrl.Contains("/Product/Create"))
            {
                return "Vẫn ở trang tạo sản phẩm (có thể có lỗi)";
            }

            return $"Trang hiện tại: {currentUrl}";
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