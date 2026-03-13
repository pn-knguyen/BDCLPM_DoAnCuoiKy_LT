using DoAnCuoiKy.Models;
using NUnit.Framework;
using OfficeOpenXml;

namespace DoAnCuoiKy
{
    public class ExcelDataProvider
    {
        private const string ExcelFilePath = @"C:\Users\phamn\OneDrive\Hoc ky 7\Bao dam chat luong phan mem\DoAn\2117_Functional_Testcase.xlsx";

        static ExcelDataProvider()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy");
        }

        public static IEnumerable<TestCaseData> GetTestCases(string sheetName, string testCaseFilter)
        {
            var testCases = new List<TestCaseData>();

            if (!File.Exists(ExcelFilePath))
            {
                TestContext.Out.WriteLine($"ERROR: Excel file not found: {ExcelFilePath}");
                return testCases;
            }

            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[sheetName];

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
                    AddTestCase(testCases, currentTcId, objective, expected, steps, startRow, testCaseFilter);

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

            AddTestCase(testCases, currentTcId, objective, expected, steps, startRow, testCaseFilter);

            TestContext.Out.WriteLine($"Loaded {testCases.Count} test cases");

            return testCases;
        }

        public static void AddTestCase(
            List<TestCaseData> list,
            string tcId,
            string? objective,
            string? expected,
            List<TestStep> steps,
            int startRow,
            string testCaseFilter)
        {
            if (!string.IsNullOrEmpty(tcId) && steps.Count > 0 && tcId.StartsWith(testCaseFilter))
            {
                list.Add(new TestCaseData(tcId, objective, steps.ToList(), expected, startRow));
            }
        }

        public static void ClearOldResults(List<TestStep> steps, string sheetName)
        {
            using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
            var worksheet = package.Workbook.Worksheets[sheetName];

            if (worksheet == null) return;

            foreach (var step in steps)
            {
                worksheet.Cells[step.ExcelRow, 11].Value = "";
                worksheet.Cells[step.ExcelRow, 12].Value = "";
            }

            package.Save();
        }

        public static void WriteTestResults(List<TestStep> steps, string actual, string result, string sheetName)
        {
            try
            {
                using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
                var worksheet = package.Workbook.Worksheets[sheetName];

                if (worksheet == null) return;

                var lastStep = steps.Last();

                int row = lastStep.ExcelRow;

                int actualCol = 11;
                int resultCol = 12;

                var actualCell = worksheet.Cells[row, actualCol];
                var resultCell = worksheet.Cells[row, resultCol];

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
}