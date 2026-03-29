using DoAnCuoiKy.Models;
using NUnit.Framework;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DoAnCuoiKy
{
    public class ExcelDataProvider
    {
        private const string ExcelFileEnvVar = "BDCLPM_EXCEL_PATH";
        private const string DefaultExcelFileName = "2117_Functional_Testcase.xlsx";
        public const string PlaceholderTestCaseId = "__MISSING_OR_EMPTY_TEST_DATA__";
        private static readonly object ExcelFileLock = new();

        private static string ExcelFilePath => ResolveExcelFilePath();

        static ExcelDataProvider()
        {
            ExcelPackage.License.SetNonCommercialPersonal("DoAnCuoiKy");
        }

        private static string ResolveExcelFilePath()
        {
            var envPath = Environment.GetEnvironmentVariable(ExcelFileEnvVar);
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                return envPath;
            }

            // Walk up from the test bin folder to project/repo roots and try common locations.
            var current = AppContext.BaseDirectory;
            for (int i = 0; i < 6 && !string.IsNullOrWhiteSpace(current); i++)
            {
                var candidateInCurrent = Path.Combine(current, DefaultExcelFileName);
                if (File.Exists(candidateInCurrent))
                {
                    return candidateInCurrent;
                }

                var candidateInTestData = Path.Combine(current, "TestData", DefaultExcelFileName);
                if (File.Exists(candidateInTestData))
                {
                    return candidateInTestData;
                }

                current = Directory.GetParent(current)?.FullName ?? string.Empty;
            }

            return Path.Combine(AppContext.BaseDirectory, DefaultExcelFileName);
        }

        public static IEnumerable<TestCaseData> GetTestCases(string sheetName, string testCaseFilter)
        {
            var testCases = new List<TestCaseData>();

            if (!File.Exists(ExcelFilePath))
            {
                TestContext.Out.WriteLine($"ERROR: Excel file not found: {ExcelFilePath}");
                TestContext.Out.WriteLine($"TIP: Set environment variable {ExcelFileEnvVar} to your Excel test file path.");
                testCases.Add(new TestCaseData(
                    PlaceholderTestCaseId,
                    "Missing Excel data file",
                    new List<TestStep>(),
                    $"Provide Excel file via {ExcelFileEnvVar}",
                    0));
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

                var stepCellText = worksheet.Cells[row, 7].Text;
                if (TryParseStepNumber(stepCellText, out int stepNum))
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

            if (testCases.Count == 0)
            {
                testCases.Add(new TestCaseData(
                    PlaceholderTestCaseId,
                    $"No test cases found for filter {testCaseFilter}",
                    new List<TestStep>(),
                    "Check sheet name/filter in test source",
                    0));
            }

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
            if (string.IsNullOrEmpty(tcId) || !tcId.StartsWith(testCaseFilter))
            {
                return;
            }

            if (steps.Count == 0 && startRow > 0)
            {
                // Keep the test discoverable and writable even if the step-number cell is malformed.
                steps.Add(new TestStep
                {
                    StepNumber = 0,
                    StepAction = string.Empty,
                    TestData = string.Empty,
                    ExcelRow = startRow
                });
            }

            if (steps.Count > 0)
            {
                list.Add(new TestCaseData(tcId, objective, steps.ToList(), expected, startRow));
            }
        }

        private static bool TryParseStepNumber(string? rawValue, out int stepNum)
        {
            stepNum = 0;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            if (int.TryParse(rawValue.Trim(), out stepNum))
            {
                return true;
            }

            var match = Regex.Match(rawValue, @"\d+");
            return match.Success && int.TryParse(match.Value, out stepNum);
        }

        public static void ClearOldResults(List<TestStep> steps, string sheetName)
        {
            try
            {
                lock (ExcelFileLock)
                {
                    ExecuteWithRetry(() =>
                    {
                        using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
                        var worksheet = package.Workbook.Worksheets[sheetName];

                        if (worksheet == null) return;

                        foreach (var step in steps)
                        {
                            worksheet.Cells[step.ExcelRow, 11].Value = "";
                            worksheet.Cells[step.ExcelRow, 13].Value = "";

                            var resultCell = worksheet.Cells[step.ExcelRow, 12];
                            resultCell.Value = "";
                            resultCell.Style.Fill.PatternType = ExcelFillStyle.None;

                            worksheet.Cells[step.ExcelRow, 11].Style.Fill.PatternType = ExcelFillStyle.None;
                            worksheet.Cells[step.ExcelRow, 13].Style.Fill.PatternType = ExcelFillStyle.None;
                        }

                        package.Save();
                    });
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"WARN clearing old Excel results: {ex.Message}");
            }
        }

        public static void WriteTestResults(List<TestStep> steps, string actual, string result, string sheetName, string? notes = null)
        {
            try
            {
                lock (ExcelFileLock)
                {
                    using var package = new ExcelPackage(new FileInfo(ExcelFilePath));
                    var worksheet = package.Workbook.Worksheets[sheetName];

                    if (worksheet == null) return;

                    var lastStep = steps.Last();

                    int row = lastStep.ExcelRow;

                    int actualCol = 11;
                    int resultCol = 12;
                    int notesCol = 13;

                    var actualCell = worksheet.Cells[row, actualCol];
                    var resultCell = worksheet.Cells[row, resultCol];
                    var notesCell = worksheet.Cells[row, notesCol];

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

                    if (notesCell.Merge)
                    {
                        var mergeAddress = worksheet.MergedCells[row, notesCol];
                        var range = worksheet.Cells[mergeAddress];
                        notesCell = range;
                    }

                    actualCell.Value = actual;
                    resultCell.Value = result;

                    notesCell.Value = notes ?? string.Empty;

                    bool isPass = string.Equals(result, "Pass", StringComparison.OrdinalIgnoreCase);

                    ApplyResultColor(actualCell, isPass);
                    ApplyResultColor(resultCell, isPass);
                    ApplyResultColor(notesCell, isPass);

                    ExecuteWithRetry(() => package.Save());

                    TestContext.Out.WriteLine($"Excel updated at {actualCell.Address}");
                }
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"ERROR writing Excel: {ex.Message}");
            }
        }

        private static void ApplyResultColor(ExcelRange cell, bool isPass)
        {
            if (isPass)
            {
                cell.Style.Fill.PatternType = ExcelFillStyle.None;
                return;
            }

            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
        }

        private static void ExecuteWithRetry(Action action, int maxAttempts = 5, int delayMs = 250)
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
                    TestContext.Out.WriteLine($"Excel IO retry {attempt}/{maxAttempts}: {ex.Message}");
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

            throw lastException ?? new InvalidOperationException("Unknown Excel write error.");
        }
    }
}