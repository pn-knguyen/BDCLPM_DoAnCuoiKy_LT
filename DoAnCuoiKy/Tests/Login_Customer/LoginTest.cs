using DoAnCuoiKy.Models;
using DoAnCuoiKy.Pages.Login_Customer;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework.Interfaces;

namespace DoAnCuoiKy.Tests.Login_Customer
{
    public class LoginTest
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private LoginPage loginPage;
        private const string SheetName = "Test Cases CUS";
        private const string TestCaseFilter = "F1.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            loginPage = new LoginPage(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Debugger.IsAttached)
                {
                    Thread.Sleep(10000);
                }
                else
                {
                    var status = TestContext.CurrentContext.Result.Outcome.Status;
                    if (status == TestStatus.Failed)
                        Thread.Sleep(5000);
                    else
                        Thread.Sleep(1000);
                }
            }
            catch
            {
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        [Test, TestCaseSource(typeof(ExcelDataProvider), nameof(ExcelDataProvider.GetTestCases), new object[] { "Test Cases CUS", "F1.1_" })]
        public void LoginCustomerTest(string tcId, string? objective, List<TestStep> steps, string? expectedResult, int startRow)
        {
            TestContext.Out.WriteLine($"Test Case ID: {tcId}");
            TestContext.Out.WriteLine($"Objective: {objective}");
            TestContext.Out.WriteLine();
            ExcelDataProvider.ClearOldResults(steps, SheetName);

            string data = string.Empty;
            // determine which field to validate based on objective similar to register
            objective = objective?.ToLower() ?? "";
            if (objective.Contains("email")) data = "Email";
            else if (objective.Contains("mật khẩu")) data = "Password";

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                TestStep? nextStep = i < steps.Count - 1 ? steps[i + 1] : null;
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                ExecuteStep(step, nextStep);
            }

            string actualResult = GetActualResult(data);

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

        private void ExecuteStep(TestStep step, TestStep? nextStep)
        {
            string action = step.StepAction?.ToLower() ?? "";
            string data = step.TestData ?? "";
            if (action.Contains("truy cập") || action.Contains("open"))
                loginPage.Navigate(data);
            else if (action.Contains("trang đăng nhập"))
                loginPage.ClickToLoginPage();
            else if (action.Contains("email"))
                loginPage.EnterEmail(data);
            else if (action.Contains("mật khẩu"))
                loginPage.EnterPassword(data);
            else if (action.Contains("ghi nhớ"))
                loginPage.ClickRememberMe();
            else if (action.Contains("đăng nhập") || action.Contains("login"))
                loginPage.ClickLogin();
        }

        private string GetActualResult(string data)
        {
            // If redirected to homepage assume login success
            var url = driver.Url;
            if (url == "http://localhost:5167/")
                return "Đăng nhập thành công";

            // check validation (html5) on specific field
            try
            {
                if (!string.IsNullOrEmpty(data))
                {
                    var validation = loginPage.GetValidationMessage(data);
                    if (!string.IsNullOrEmpty(validation))
                        return $"Hiển thị thông báo: {validation}";
                }
            }
            catch { }

            // check general error
            var error = loginPage.GetErrorMessage();
            if (!string.IsNullOrEmpty(error))
                return $"Hiển thị thông báo: {error}";

            return "";
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

        private string? CaptureFailureScreenshot(string tcId)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var folder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots", "Login", "Customer");
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
