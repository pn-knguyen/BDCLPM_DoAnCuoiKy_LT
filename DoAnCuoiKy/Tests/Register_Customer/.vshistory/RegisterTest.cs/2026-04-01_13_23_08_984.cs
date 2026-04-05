using DoAnCuoiKy.Pages.Register_Customer;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Tests.Register_Customer
{
    public class RegisterTest
    {
        public EdgeDriver driver;
        public WebDriverWait wait;
        private RegisterPage registerPage;
        private const string SheetName = "Test Cases CUS";
        private const string TestCaseFilter = "F5.1_";

        [SetUp]
        public void Setup()
        {
            driver = new EdgeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            colorPage = new Create(driver, wait);
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}