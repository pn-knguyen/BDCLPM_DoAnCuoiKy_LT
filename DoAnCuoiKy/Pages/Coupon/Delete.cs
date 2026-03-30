using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages.Coupon
{
    public class Delete
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Delete(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public void ConfirmDelete()
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']"))); //
            btn.Click();
            // Xử lý Alert xác nhận từ trình duyệt nếu có
            try { _driver.SwitchTo().Alert().Accept(); } catch { }
        }

        public bool IsDeleteDisabled()
        {
            // Kiểm tra xem nút xóa có bị disabled do coupon đã có đơn hàng không
            return _driver.FindElements(By.CssSelector("button[disabled]")).Count > 0;
        }
    }
}