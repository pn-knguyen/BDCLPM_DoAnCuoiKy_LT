using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Category;

namespace DoAnCuoiKy.Tests.Category
{
    [NonParallelizable]
    public class EditTests : CategoryTestBase
    {
        private const string TestCaseFilter = "F4.2_";

        private Edit _categoryPage = null!;
        private bool _modalOpened;
        private bool _submitted;
        private bool _cancelled;

        [SetUp]
        public void SetupEditPage()
        {
            _categoryPage = new Edit(Driver, Wait);
            _modalOpened = false;
            _submitted = false;
            _cancelled = false;
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { SheetName, TestCaseFilter })]
        public void EditCategoryTestCase(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
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
                if (!string.IsNullOrWhiteSpace(data))
                    _categoryPage.Navigate(data);
                else
                    _categoryPage.Navigate(GetCategoryIndexUrl());
            }

            else if (action.Contains("chọn danh mục cần sửa"))
            {
                // Chỉ chọn ngữ cảnh, thao tác mở modal sửa ở bước tiếp theo.
            }

            else if (action.Contains("nút sửa") || action.Contains("chọn sửa") || action.Contains("icon sửa") || action.Contains("edit"))
            {
                _categoryPage.ClickEditCategory(data);
                _modalOpened = true;
            }

            else if (action.Contains("để trống tên"))
                _categoryPage.EnterCategoryName(string.Empty);

            else if (action.Contains("thay đổi tên") || action.Contains("đổi tên") || action.Contains("tên danh mục") || action.Contains("category name"))
                _categoryPage.EnterCategoryName(data);

            else if (action.Contains("mô tả"))
            {
                // Category hiện tại không có trường mô tả trong form sửa.
            }

            else if (action.Contains("lưu") || action.Contains("submit") || action.Contains("cập nhật"))
            {
                _categoryPage.Submit();
                _submitted = true;

                AcceptAlertIfPresent();
            }

            else if (action.Contains("hủy") || action.Contains("cancel"))
            {
                _categoryPage.Cancel();
                _cancelled = true;
            }
        }

        private string GetActualResult()
        {
            if (!string.IsNullOrWhiteSpace(LastDialogMessage))
                return LastDialogMessage;

            if (_cancelled && !_categoryPage.IsModalOpen())
                return "Hủy thao tác sửa quay lại trang quản lý danh mục";

            if (_modalOpened && !_submitted && _categoryPage.IsModalOpen())
                return "Giao diện sửa danh mục hiển thị đúng";

            if (_submitted && !_categoryPage.IsModalOpen() && _categoryPage.IsCategoryTableVisible())
                return "Cập nhật danh mục thành công";

            if (_submitted && _categoryPage.IsModalOpen())
                return "Cập nhật danh mục chưa thành công (modal vẫn mở)";

            string success = _categoryPage.GetSuccessMessage();
            if (!string.IsNullOrWhiteSpace(success))
                return success;

            string validation = _categoryPage.GetValidationErrors();
            if (!string.IsNullOrWhiteSpace(validation))
                return $"Lỗi validation: {validation}";

            if (_categoryPage.IsModalOpen())
                return "Modal sửa danh mục đang mở";

            if (_categoryPage.IsCategoryTableVisible() && GetCurrentUrl().Contains("/Category/Index"))
                return "Trang quản lý danh mục hiển thị";

            return $"Trang hiện tại: {GetCurrentUrl()}";
        }
    }
}
