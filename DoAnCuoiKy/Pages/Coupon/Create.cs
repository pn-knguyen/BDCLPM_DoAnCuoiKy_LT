using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace DoAnCuoiKy.Pages
{
    public class CreateCoupon
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public CreateCoupon(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement Wait(By locator)
        {
            return wait.Until(d => d.FindElement(locator));
        }

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void EnterUsername(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var username = Wait(By.Id("Username"));
                username.Clear();
                username.SendKeys(data);
            }
        }

        public void EnterPassword(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var password = Wait(By.Id("Password"));
                password.Clear();
                password.SendKeys(data);
            }
        }

        public void ClickLogin()
        {
            // Sử dụng XPath để tìm nút đăng nhập dựa trên text
            Wait(By.XPath("//button[contains(.,'Đăng nhập')]")).Click();
        }

        public void OpenCouponManagement()
        {
            Wait(By.LinkText("Quản lý Coupon")).Click();
        }

        public void ClickCreateCoupon()
        {
            Wait(By.LinkText("Tạo Coupon Mới")).Click();
        }

        public void EnterCode(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var code = Wait(By.Id("Code"));
                code.Clear();
                code.SendKeys(data);
            }
        }

        public void EnterDiscountAmount(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var discountAmount = Wait(By.Id("DiscountAmount"));
                discountAmount.Clear();
                discountAmount.SendKeys(data);
            }
        }

        public void EnterExpiryDate(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var expiryDate = Wait(By.Id("ExpiryDate"));

                // Chuyển đổi định dạng từ Excel (vd: 04/29/2026 10:30:30 PM) sang đối tượng DateTime
                if (DateTime.TryParse(data, out DateTime parsedDate))
                {
                    // Định dạng lại theo chuẩn ISO mà input type="datetime-local" hỗ trợ
                    string isoFormat = parsedDate.ToString("yyyy-MM-ddTHH:mm:ss");

                    // Sử dụng JavaScriptExecutor để gán trực tiếp giá trị vào thuộc tính 'value'
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("arguments[0].value = arguments[1];", expiryDate, isoFormat);

                    // Kích hoạt event 'change' để UI nhận diện sự thay đổi (nếu hệ thống có dùng JS framework như React/Vue/jQuery)
                    js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", expiryDate);
                }
                else
                {
                    // Fallback: Dùng SendKeys thông thường nếu dữ liệu không phải ngày tháng hợp lệ
                    expiryDate.Clear();
                    expiryDate.SendKeys(data);
                }
            }
        }

        public void Submit()
        {
            // Sử dụng XPath text để xác định chính xác nút "Tạo Coupon"
            Wait(By.XPath("//button[contains(.,'Tạo Coupon')]")).Click();
        }

        public string GetSuccessMessage()
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".border-green-400"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                return msg?.Text ?? "";
            }
            catch
            {
                return "";
            }
        }

        public string GetValidationErrors()
        {
            try
            {
                var errors = driver.FindElements(By.CssSelector(".field-validation-error, #DiscountAmount-error, .text-red-500"));

                // Thêm .Distinct() để loại bỏ các câu báo lỗi giống nhau
                var result = string.Join(", ", errors
                    .Select(e => e.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct());

                return result;
            }
            catch
            {
                return "";
            }
        }
        public void ClickCancel()
        {
            // Tìm và click vào nút "Hủy" (có thể là thẻ <a> hoặc <button>)
            Wait(By.XPath("//*[contains(text(), 'Hủy')]")).Click();
        }
        public string GetAlertTextAndAccept()
        {
            try
            {
                // Tự viết hàm chờ Alert thay vì dùng ExpectedConditions
                IAlert alert = wait.Until(d =>
                {
                    try
                    {
                        return d.SwitchTo().Alert();
                    }
                    catch (NoAlertPresentException)
                    {
                        return null; // Trả về null để WebDriverWait tiếp tục chờ
                    }
                });

                if (alert != null)
                {
                    string alertText = alert.Text;
                    alert.Accept(); // Nhấn OK để đóng Alert
                    return alertText;
                }

                return "";
            }
            catch (WebDriverTimeoutException)
            {
                // Quá thời gian chờ mà không có Alert nào
                return "";
            }
        }
    }
}