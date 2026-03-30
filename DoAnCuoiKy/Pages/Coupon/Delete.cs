using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages
{
    public class DeleteCoupon
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public DeleteCoupon(IWebDriver driver, WebDriverWait wait)
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
            Wait(By.XPath("//button[contains(.,'Đăng nhập')]")).Click();
        }

        public void OpenCouponManagement()
        {
            Wait(By.LinkText("Quản lý Coupon")).Click();
        }

        // Bước 1: Click nút xóa ở trang danh sách (Index)
        public void ClickDeleteByCode(string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode)) return;

            string safeCode = couponCode.Replace("'", "\"");
            // Giả định bảng danh sách có chứa text Mã Coupon và link Xóa
            string xpath = $"//td[contains(normalize-space(), '{safeCode}')]/ancestor::tr//a[contains(@href, '/Coupon/Delete/')]";

            var deleteBtn = wait.Until(d => d.FindElement(By.XPath(xpath)));
            deleteBtn.Click();
        }

        // Bước 2: Nhấn "Xác nhận xóa" trên trang Delete.cshtml
        public void ClickConfirmDeleteButton()
        {
            // Bắt nút Submit có chữ "Xác nhận xóa"
            Wait(By.XPath("//button[contains(.,'Xác nhận xóa')]")).Click();
        }

        // Bấm nút Hủy trên trang Delete.cshtml
        public void ClickCancel()
        {
            Wait(By.XPath("//a[contains(.,'Hủy')]")).Click();
        }

        // Kiểm tra xem nút Xóa có bị disable do Coupon đã có đơn hàng không
        public bool IsDeleteButtonDisabled()
        {
            try
            {
                // Tìm nút "Không thể xóa" khi hasOrders = true
                var btn = driver.FindElement(By.XPath("//button[contains(.,'Không thể xóa')]"));
                return btn.Displayed && !btn.Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        // Lấy thông báo cảnh báo/lỗi (Vd: "Không thể xóa!" hoặc "Cảnh báo!")
        public string GetWarningMessage()
        {
            try
            {
                // Bắt class CSS của thẻ div chứa thông báo đỏ hoặc vàng
                var el = wait.Until(d => d.FindElement(By.CssSelector(".bg-red-100, .bg-yellow-100, .bg-red-100.border.border-red-400.text-red-700.px-4.py-3.rounded.relative.mb-4, .h2")));
                return el.Text;
            }
            catch
            {
                return "";
            }
        }

        // Lấy thông báo xóa thành công dựa vào file Selenium IDE (.border-green-400)
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

        // Xử lý Alert javascript (xuất hiện do thuộc tính onclick="return confirm(...)")
        public void AcceptDeleteConfirmationAlert()
        {
            var alert = wait.Until(d =>
            {
                try
                {
                    return d.SwitchTo().Alert();
                }
                catch (NoAlertPresentException)
                {
                    return null;
                }
            });

            alert.Accept();
        }

        public void DismissDeleteConfirmationAlert()
        {
            var alert = wait.Until(d =>
            {
                try
                {
                    return d.SwitchTo().Alert();
                }
                catch (NoAlertPresentException)
                {
                    return null;
                }
            });

            alert.Dismiss();
        }

        // Kiểm tra xem Coupon còn tồn tại ngoài Index không (dùng để Assert sau khi xóa)
        public bool HasCouponByCode(string couponCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(couponCode)) return false;

                string safeCode = couponCode.Replace("'", "\"");
                string xpath = $"//td[contains(normalize-space(), '{safeCode}')]";
                return driver.FindElements(By.XPath(xpath)).Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}