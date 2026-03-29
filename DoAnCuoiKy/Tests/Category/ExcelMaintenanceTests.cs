using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DoAnCuoiKy.Tests.Category
{
    [NonParallelizable]
    public class ExcelMaintenanceTests
    {
        private const string SheetName = "Test Cases AD";

        [Test]
        [Explicit]
        public void NormalizeCategoryDataSetForCurrentSource()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy");

            string excelPath = ResolveExcelPath();
            Assert.That(File.Exists(excelPath), Is.True, $"Excel file not found: {excelPath}");

            using var package = new ExcelPackage(new FileInfo(excelPath));
            var sheet = package.Workbook.Worksheets[SheetName];
            Assert.That(sheet, Is.Not.Null, $"Worksheet {SheetName} not found.");

            var expectedByTc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["F4.1_00"] = "Giao diện thêm danh mục hiển thị đúng",
                ["F4.1_01"] = "Thêm danh mục thành công",
                ["F4.1_02"] = "chưa thành công",
                ["F4.1_03"] = "Tên danh mục đã tồn tại",
                ["F4.1_04"] = "Thêm danh mục thành công",
                ["F4.1_05"] = "Thêm danh mục thành công",
                ["F4.1_06"] = "Thêm danh mục thành công",
                ["F4.1_07"] = "Form được đóng không thêm danh mục mới",

                ["F4.2_00"] = "Giao diện sửa danh mục hiển thị đúng",
                ["F4.2_01"] = "Cập nhật danh mục thành công",
                ["F4.2_02"] = "chưa thành công",
                ["F4.2_03"] = "Tên danh mục đã tồn tại",
                ["F4.2_04"] = "Cập nhật danh mục thành công",
                ["F4.2_05"] = "Cập nhật danh mục thành công",
                ["F4.2_06"] = "Cập nhật danh mục thành công",
                ["F4.2_07"] = "Hủy thao tác sửa quay lại trang quản lý danh mục",
                ["F4.2_08"] = "Cập nhật danh mục thành công",
                ["F4.2_09"] = "Cập nhật danh mục thành công",
                ["F4.2_10"] = "Giao diện sửa danh mục hiển thị đúng",

                ["F4.3_01"] = "Không thể xóa danh mục đang được sử dụng trong sản phẩm",
                ["F4.3_02"] = "Không thể xóa danh mục đang được sử dụng trong sản phẩm",
                ["F4.3_03"] = "Bạn có chắc chắn muốn xóa danh mục này?",
                ["F4.3_04"] = "Bạn có chắc chắn muốn xóa danh mục này?",
                ["F4.3_05"] = "Bạn có chắc chắn muốn xóa danh mục này?"
            };

            var testDataByTcAndStep = new Dictionary<(string Tc, int Step), string>(new TcStepComparer())
            {
                [("F4.1_01", 3)] = "{AUTO_UNIQUE}",
                [("F4.1_04", 3)] = "{AUTO_UNIQUE_LONG}",
                [("F4.1_05", 3)] = "{AUTO_UNIQUE_SPECIAL}",
                [("F4.1_06", 3)] = "{AUTO_UNIQUE}"
            };

            string currentTc = string.Empty;
            int rowCount = sheet!.Dimension!.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string tcCell = sheet.Cells[row, 4].Text.Trim();
                if (!string.IsNullOrEmpty(tcCell))
                {
                    currentTc = tcCell;

                    if (expectedByTc.TryGetValue(currentTc, out string? expected))
                    {
                        sheet.Cells[row, 10].Value = expected;
                    }
                }

                if (IsCategoryTc(currentTc))
                {
                    if (int.TryParse(sheet.Cells[row, 7].Text, out int stepNum)
                        && testDataByTcAndStep.TryGetValue((currentTc, stepNum), out string? newData))
                    {
                        sheet.Cells[row, 9].Value = newData;
                    }

                    // Reset old execution artifacts so rerun reflects current dataset.
                    sheet.Cells[row, 11].Value = string.Empty;
                    sheet.Cells[row, 12].Value = string.Empty;
                    sheet.Cells[row, 13].Value = string.Empty;

                    sheet.Cells[row, 11].Style.Fill.PatternType = ExcelFillStyle.None;
                    sheet.Cells[row, 12].Style.Fill.PatternType = ExcelFillStyle.None;
                    sheet.Cells[row, 13].Style.Fill.PatternType = ExcelFillStyle.None;
                }
            }

            ExecuteWithRetry(() => package.Save());

            TestContext.Out.WriteLine($"Category dataset normalized in: {excelPath}");
        }
        private static void ExecuteWithRetry(Action action, int maxAttempts = 5, int delayMs = 500)
        {
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    action();
                    return;
                }
                catch (IOException ex)
                {
                    lastException = ex;
                    TestContext.Out.WriteLine($"Excel save retry {attempt}/{maxAttempts}: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    lastException = ex;
                    TestContext.Out.WriteLine($"Excel save retry {attempt}/{maxAttempts}: {ex.Message}");
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(delayMs * attempt);
                }
            }

            throw lastException ?? new InvalidOperationException("Excel save failed after retries.");
        }


        private static bool IsCategoryTc(string tcId)
        {
            return tcId.StartsWith("F4.1_", StringComparison.OrdinalIgnoreCase)
                || tcId.StartsWith("F4.2_", StringComparison.OrdinalIgnoreCase)
                || tcId.StartsWith("F4.3_", StringComparison.OrdinalIgnoreCase)
                || tcId.StartsWith("F4.0_", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveExcelPath()
        {
            string? fromEnv = Environment.GetEnvironmentVariable("BDCLPM_EXCEL_PATH");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv;
            }

            string candidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "2117_Functional_Testcase.xlsx"));
            return candidate;
        }

        private sealed class TcStepComparer : IEqualityComparer<(string Tc, int Step)>
        {
            public bool Equals((string Tc, int Step) x, (string Tc, int Step) y)
            {
                return x.Step == y.Step && string.Equals(x.Tc, y.Tc, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode((string Tc, int Step) obj)
            {
                return HashCode.Combine(obj.Step, obj.Tc.ToLowerInvariant());
            }
        }
    }
}
