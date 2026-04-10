using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Category;

namespace DoAnCuoiKy.Tests.Category
{
    [NonParallelizable]
    public class ListingTests : CategoryTestBase
    {
        private const string TestCaseFilter = "F4.4_";

        private CategoryIndexPage _categoryIndexPage = null!;
        private string _selectedCategory = string.Empty;
        private bool _navigatedToIndex;

        [SetUp]
        public void SetupCategoryListing()
        {
            _categoryIndexPage = new CategoryIndexPage(Driver, Wait);
            _selectedCategory = string.Empty;
            _navigatedToIndex = false;
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void CategoryListingTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            if (tcId == ExcelDataProvider.PlaceholderTestCaseId)
            {
                Assert.Ignore($"Missing Excel data for {TestCaseFilter}. Provide {expectedResult}.");
            }

            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();

            ExcelDataProvider.ClearOldResults(steps, SheetName);

            string actualResult;
            bool testPassed;
            string notes = string.Empty;

            try
            {
                foreach (var step in steps)
                {
                    TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                    ExecuteStep(step);
                }

                if (!_navigatedToIndex)
                {
                    _categoryIndexPage.Navigate(GetCategoryIndexUrl());
                    _navigatedToIndex = true;
                }

                actualResult = GetActualResult();
                testPassed = IsExpectedMatched(expectedResult, actualResult);
            }
            catch (Exception ex)
            {
                actualResult = $"Exception: {ex.Message}";
                testPassed = false;
            }

            if (!testPassed)
            {
                notes = $"Screenshot: {CaptureFailureScreenshot(tcId)}";
            }

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Expected: {expectedResult}");
            TestContext.Out.WriteLine($"Actual: {actualResult}");
            TestContext.Out.WriteLine($"Result: {(testPassed ? "PASS" : "FAIL")}");

            ExcelDataProvider.WriteTestResults(
                steps,
                actualResult,
                testPassed ? "Pass" : "Fail",
                SheetName,
                notes);

            Assert.That(testPassed, Is.True, actualResult);
        }

        private void ExecuteStep(TestStep step)
        {
            string action = step.StepAction?.ToLower() ?? string.Empty;
            string data = step.TestData ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action) && string.IsNullOrWhiteSpace(data))
                return;

            if (ExecuteLoginStep(action, data))
                return;

            if (action.Contains("quản lý danh mục"))
            {
                _categoryIndexPage.Navigate(GetCategoryIndexUrl());
                _navigatedToIndex = true;
            }

            else if (action.Contains("thêm danh mục") || action.Contains("add new"))
                _categoryIndexPage.ClickAddNewCategory();

            else if (action.Contains("chọn danh mục") || action.Contains("hiển thị danh mục"))
                _selectedCategory = ExtractCategoryName(action, data);

            else if (action.Contains("nút \"xóa\"") || action.Contains("icon xóa") || action.Contains("nhấn nút \"xoá\"") || action.Contains("nhấn nút \"xóa\""))
                _categoryIndexPage.ClickDeleteByName(_selectedCategory);

            else if (action.Contains("xác nhận") || action.Contains("ok"))
                AcceptAlertIfPresent();

            else if (action.Contains("hủy") || action.Contains("cancel"))
                DismissAlertIfPresent();
        }

        private string GetActualResult()
        {
            if (!string.IsNullOrWhiteSpace(LastDialogMessage))
                return LastDialogMessage;

            if (_categoryIndexPage.CurrentUrl.Contains("/Category/Index") && _categoryIndexPage.IsTableVisible())
            {
                var heading = _categoryIndexPage.GetPageHeading();
                if (heading.Contains("Quản lý danh mục"))
                {
                    return "Trang danh sách danh mục hiển thị đúng";
                }
            }

            if (_categoryIndexPage.IsModalOpen())
                return "Modal thêm danh mục đã mở";

            return $"Trang hiện tại: {Driver.Url}";
        }

        private static string ExtractCategoryName(string action, string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                return data.Trim();
            }

            int firstQuote = action.IndexOf('"');
            if (firstQuote >= 0)
            {
                int secondQuote = action.IndexOf('"', firstQuote + 1);
                if (secondQuote > firstQuote)
                {
                    return action.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim();
                }
            }

            return string.Empty;
        }
    }
}
