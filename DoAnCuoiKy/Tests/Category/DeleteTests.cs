using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Category;

namespace DoAnCuoiKy.Tests.Category
{
    [NonParallelizable]
    public class DeleteTests : CategoryTestBase
    {
        private const string TestCaseFilter = "F4.3_";

        private CategoryIndexPage _categoryIndexPage = null!;
        private string _selectedCategory = string.Empty;
        private bool _confirmedDelete;
        private bool _cancelledDelete;

        [SetUp]
        public void SetupDeletePage()
        {
            _categoryIndexPage = new CategoryIndexPage(Driver, Wait);
            _selectedCategory = string.Empty;
            _confirmedDelete = false;
            _cancelledDelete = false;
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void DeleteCategoryTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            if (tcId == ExcelDataProvider.PlaceholderTestCaseId)
            {
                Assert.Ignore($"Missing Excel data for {TestCaseFilter}. Provide {expectedResult}.");
            }

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
            bool testPassed = IsExpectedMatched(expectedResult, actualResult);

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Expected: {expectedResult}");
            TestContext.Out.WriteLine($"Actual: {actualResult}");
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
            string action = step.StepAction?.ToLower() ?? string.Empty;
            string data = step.TestData ?? string.Empty;

            if (ExecuteLoginStep(action, data))
                return;

            if (action.Contains("quản lý danh mục"))
            {
                _categoryIndexPage.Navigate(GetCategoryIndexUrl());
            }
            else if (action.Contains("chọn danh mục") || action.Contains("hiển thị danh mục"))
            {
                _selectedCategory = ExtractCategoryName(action, data);
            }
            else if (action.Contains("nút \"xóa\"") || action.Contains("icon xóa") || action.Contains("nhấn nút \"xoá\"") || action.Contains("nhấn nút \"xóa\""))
            {
                _categoryIndexPage.ClickDeleteByName(_selectedCategory);
            }
            else if (action.Contains("xác nhận") || action.Contains("ok"))
            {
                _confirmedDelete = true;

                // Alert xác nhận xóa từ confirm().
                AcceptAlertIfPresent();

                // Alert lỗi nghiệp vụ (nếu có) từ handleResponse().
                Thread.Sleep(500);
                AcceptAlertIfPresent();
            }
            else if (action.Contains("hủy") || action.Contains("cancel"))
            {
                _cancelledDelete = true;
                DismissAlertIfPresent();
            }
        }

        private string GetActualResult()
        {
            if (!string.IsNullOrWhiteSpace(LastDialogMessage))
                return LastDialogMessage;

            if (_cancelledDelete)
                return "Đã hủy xóa danh mục";

            if (_confirmedDelete)
            {
                bool stillExists = _categoryIndexPage.HasCategoryByName(_selectedCategory);
                return stillExists
                    ? "Không thể xóa danh mục đang được sử dụng trong sản phẩm"
                    : "Xóa danh mục thành công";
            }

            if (_categoryIndexPage.CurrentUrl.Contains("/Category/Index") && _categoryIndexPage.IsTableVisible())
                return "Trang danh sách danh mục hiển thị đúng";

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