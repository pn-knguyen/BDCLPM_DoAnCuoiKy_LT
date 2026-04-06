using DoAnCuoiKy.Pages.Color;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Tests.Color
{
    public class EditTest
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private Edit colorPage;
        private const string SheetName = "Test Cases AD";
        private const string TestCaseFilter = "F5.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            colorPage = new Edit(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}