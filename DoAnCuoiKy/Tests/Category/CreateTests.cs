using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Category;

namespace DoAnCuoiKy.Tests.Category
{
    [NonParallelizable]
    public class CreateTests : CategoryTestBase
    {
        private const string TestCaseFilter = "F4.1_";

        private Create _categoryPage = null!;
        private bool _modalOpened;
        private bool _submitted;
        private bool _cancelled;

        [SetUp]
        public void SetupCreatePage()
        {
            _categoryPage = new Create(Driver, Wait);
            _modalOpened = false;
            _submitted = false;
            _cancelled = false;
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void CreateCategoryTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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

            if (ExecuteLoginStep(action, data))
                return;

            if (action.Contains("quản lý danh mục"))
            {
                if (!string.IsNullOrWhiteSpace(data))
                    _categoryPage.Navigate(data);
                else
                    _categoryPage.Navigate(GetCategoryIndexUrl());
            }

            else if (action.Contains("thêm danh mục") || action.Contains("tạo danh mục") || action.Contains("add new"))
            {
                _categoryPage.ClickCreateCategory();
                _modalOpened = true;
            }

            else if (action.Contains("để trống tên"))
                _categoryPage.EnterCategoryName(string.Empty);

            else if (action.Contains("tên danh mục") || action.Contains("category name"))
                _categoryPage.EnterCategoryName(ResolveDynamicTestData(data));

            else if (action.Contains("mô tả"))
            {
                // Category hiện tại không có trường mô tả trong UI.
            }

            else if (action.Contains("lưu") || action.Contains("submit") || action.Contains("tạo mới") || (action.Contains("nhấn nút") && action.Contains("thêm")))
            {
                _categoryPage.Submit();
                _submitted = true;

                // Validation/duplicate được trả về alert từ JS.
                AcceptAlertIfPresent();
            }

            else if (action.Contains("hủy") || action.Contains("cancel"))
            {
                _categoryPage.Cancel();
                _cancelled = true;
            }

            else if (action.Contains("check giao diện") || action.Contains("checklist"))
            {
                // Bước xác nhận giao diện, không thao tác.
            }
        }

        private string GetActualResult()
        {
            if (!string.IsNullOrWhiteSpace(LastDialogMessage))
                return LastDialogMessage;

            if (_cancelled && !_categoryPage.IsModalOpen())
                return "Form được đóng không thêm danh mục mới";

            if (_modalOpened && !_submitted && _categoryPage.IsModalOpen())
                return "Giao diện thêm danh mục hiển thị đúng";

            if (_submitted && !_categoryPage.IsModalOpen() && _categoryPage.IsCategoryTableVisible())
                return "Thêm danh mục thành công";

            if (_submitted && _categoryPage.IsModalOpen())
                return "Thêm danh mục chưa thành công (modal vẫn mở)";

            string success = _categoryPage.GetSuccessMessage();
            if (!string.IsNullOrWhiteSpace(success))
                return success;

            string validation = _categoryPage.GetValidationErrors();
            if (!string.IsNullOrWhiteSpace(validation))
                return $"Lỗi validation: {validation}";

            if (_categoryPage.IsModalOpen())
                return "Modal thêm danh mục đang mở";

            if (_categoryPage.IsCategoryTableVisible() && GetCurrentUrl().Contains("/Category/Index"))
                return "Trang quản lý danh mục hiển thị";

            return $"Trang hiện tại: {GetCurrentUrl()}";
        }
    }
}
