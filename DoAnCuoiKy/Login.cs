using NUnit.Framework;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy
{
    public class TestStep
    {
        public int StepNumber { get; set; }
        public string? StepAction { get; set; }
        public string? TestData { get; set; }
        public string? Actual { get; set; }
        public string? Result { get; set; }
        public string? Notes { get; set; }
        public int ExcelRow { get; set; }
    }

    public class LoginExcelDataProvider
    {
        private const string ExcelFilePath = @"C:\Users\phamn\BaoDamChatLuongPM\LyThuyet\B12 - Functional Test case.xlsx";
        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F1_";

        static LoginExcelDataProvider()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy");
        }

        public static IEnumerable<TestCaseData> GetTestCaseDataFromExcel()
        {
            var testCases = new List<TestCaseData>();

            if (!File.Exists(ExcelFilePath))
            {
                TestContext.Out.WriteLine($"ERROR: Excel file not found: {ExcelFilePath}");
                return testCases;
            }

            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[SheetName];

            if (worksheet?.Dimension == null)
                return testCases;

            int rowCount = worksheet.Dimension.Rows;

            string currentTcId = "";
            string? objective = "";
            string? expected = "";
            List<TestStep> steps = new();
            int startRow = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                string tcId = worksheet.Cells[row, 4].Text.Trim();

                if (!string.IsNullOrEmpty(tcId))
                {
                    AddTestCase(testCases, currentTcId, objective, expected, steps, startRow);

                    currentTcId = tcId;
                    objective = worksheet.Cells[row, 5].Text;
                    expected = worksheet.Cells[row, 10].Text;
                    steps = new List<TestStep>();
                    startRow = row;
                }

                if (int.TryParse(worksheet.Cells[row, 7].Text, out int stepNum))
                {
                    steps.Add(new TestStep
                    {
                        StepNumber = stepNum,
                        StepAction = worksheet.Cells[row, 8].Text,
                        TestData = worksheet.Cells[row, 9].Text,
                        Actual = worksheet.Cells[row, 11].Text,
                        Result = worksheet.Cells[row, 12].Text,
                        Notes = worksheet.Cells[row, 13].Text,
                        ExcelRow = row
                    });
                }
            }

            AddTestCase(testCases, currentTcId, objective, expected, steps, startRow);

            TestContext.Out.WriteLine($"Loaded {testCases.Count} test cases");

            return testCases;
        }

        public static void AddTestCase(
            List<TestCaseData> list,
            string tcId,
            string? objective,
            string? expected,
            List<TestStep> steps,
            int startRow)
        {
            if (!string.IsNullOrEmpty(tcId) && steps.Count > 0 && tcId.StartsWith(TestCaseFilter))
            {
                list.Add(new TestCaseData(tcId, objective, steps.ToList(), expected, startRow));
            }
        }

        public static void ClearOldResults(List<TestStep> steps)
        {
            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[SheetName];

            if (worksheet == null) return;

            foreach (var step in steps)
            {
                worksheet.Cells[step.ExcelRow, 11].Value = "";
                worksheet.Cells[step.ExcelRow, 12].Value = "";
            }

            package.Save();
        }

        public static void WriteTestResults(List<TestStep> steps, string actual, string result)
        {
            try
            {
                using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
                var worksheet = package.Workbook.Worksheets[SheetName];

                if (worksheet == null) return;

                var lastStep = steps.Last();

                int row = lastStep.ExcelRow;

                int actualCol = 11;
                int resultCol = 12;

                var actualCell = worksheet.Cells[row, actualCol];
                var resultCell = worksheet.Cells[row, resultCol];

                // Nếu cell nằm trong merged region thì tìm ô đầu
                if (actualCell.Merge)
                {
                    var mergeAddress = worksheet.MergedCells[row, actualCol];
                    var range = worksheet.Cells[mergeAddress];

                    actualCell = range;
                }

                if (resultCell.Merge)
                {
                    var mergeAddress = worksheet.MergedCells[row, resultCol];
                    var range = worksheet.Cells[mergeAddress];

                    resultCell = range;
                }

                actualCell.Value = actual;
                resultCell.Value = result;

                package.Save();

                TestContext.Out.WriteLine($"Excel updated at {actualCell.Address}");
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"ERROR writing Excel: {ex.Message}");
            }
        }
    }

    public class Tests
    {
        public EdgeDriver driver;
        public WebDriverWait wait;

        public const string LoginUrl = "http://localhost:5080/AdminAccount/Login";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl(LoginUrl);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }

        [Test, TestCaseSource(typeof(LoginExcelDataProvider), nameof(LoginExcelDataProvider.GetTestCaseDataFromExcel))]
        public void ExecuteLoginTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"=== Executing Test Case: {tcId} ===");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            LoginExcelDataProvider.ClearOldResults(steps);

            foreach (var step in steps)
            {
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                ExecuteStep(step);
            }

            string actualResult = GetActualResult();

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Expected: {expectedResult}");
            TestContext.Out.WriteLine($"Actual: {actualResult}");

            // Xác định test case mong đợi thành công hay thất bại
            bool expectSuccess = expectedResult?.ToLower().Contains("admin") == true ||
                                expectedResult?.ToLower().Contains("dashboard") == true ||
                                expectedResult?.ToLower().Contains("chuyển") == true;

            bool testPassed;
            if (expectSuccess)
            {
                // Test case mong đợi đăng nhập thành công
                testPassed = actualResult.ToLower().Contains("admin") || 
                           actualResult.ToLower().Contains("dashboard");
            }
            else
            {
                // Test case mong đợi đăng nhập thất bại (hiển thị lỗi)
                testPassed = !actualResult.ToLower().Contains("admin") && 
                           !actualResult.ToLower().Contains("dashboard") &&
                           (actualResult.ToLower().Contains("không đúng") ||
                            actualResult.ToLower().Contains("sai") ||
                            actualResult.ToLower().Contains("lỗi") ||
                            actualResult.Contains("login page"));
            }

            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            var lastStep = steps.Last();

            LoginExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail");

            Assert.That(testPassed, Is.True);
        }

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";

            if (action.Contains("mở") || action.Contains("navigate"))
            {
                driver.Navigate().GoToUrl(data);
            }

            else if (action.Contains("username") || action.Contains("tên"))
            {
                var username = WaitForElement(By.Id("Username"));
                username.Clear();
                username.SendKeys(data);
            }

            else if (action.Contains("password") || action.Contains("mật"))
            {
                var password = WaitForElement(By.Id("Password"));
                password.Clear();
                password.SendKeys(data);
            }

            else if (action.Contains("click") || action.Contains("login") || action.Contains("đăng nhập"))
            {
                var button = WaitForElement(By.CssSelector("button[type='submit']"));
                button.Click();
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

            // Login SUCCESS
            if (currentUrl.Contains("/Dashboard"))
            {
                return $"Redirected to Admin page: {currentUrl}";
            }

            // Login FAILED -> lấy message lỗi
            try
            {
                var error = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".font-medium"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                if (error != null)
                {
                    return error.Text;
                }
            }
            catch
            {
            }

            // nếu không có error message
            return $"Still on login page: {currentUrl}";
        }

        private string TryGetErrorMessage()
        {
            try
            {
                var error = FindElement(
                    By.ClassName("error-message"),
                    By.ClassName("alert-danger"),
                    By.CssSelector(".validation-summary-errors")
                );

                return error?.Text ?? "Login failed but no message found";
            }
            catch
            {
                return "Login failed";
            }
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