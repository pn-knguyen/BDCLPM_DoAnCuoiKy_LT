using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages.Coupon
{
    public class Edit
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Edit(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public void UpdateExpiryDate(string newDate)
        {
            var input = _wait.Until(d => d.FindElement(By.Name("ExpiryDate"))); //
            input.Clear();
            input.SendKeys(newDate);
        }

        public void SetUsedStatus(bool used)
        {
            var checkbox = _wait.Until(d => d.FindElement(By.Name("IsUsed"))); //
            if (checkbox.Selected != used) checkbox.Click();
        }

        public void Submit() => _driver.FindElement(By.XPath("//button[contains(text(), 'Cập nhật')]")).Click(); //
    }
}