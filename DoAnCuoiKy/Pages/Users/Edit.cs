using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace DoAnCuoiKy.Pages.Users
{
    public class Edit
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public Edit(IWebDriver driver, WebDriverWait wait)
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

        public void EnterEmail(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var email = Wait(By.Id("Email"));
                email.Clear();
                email.SendKeys(data);
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

        public void OpenProfile()
        {
            Wait(By.Id("user-menu-btn")).Click();
            Wait(By.LinkText("Thông tin cá nhân")).Click();
        }

        public void EnterName(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var name = Wait(By.Id("Name"));
                name.Clear();
                name.SendKeys(data);
            }
        }
        public void EnterPhone(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var phone = Wait(By.Id("Phone"));
                phone.Clear();
                phone.SendKeys(data);
            }
        }
        public void EnterBirthYear(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var birthYear = Wait(By.Id("BirthYear"));
                birthYear.Clear();
                birthYear.SendKeys(data);
            }
        }

        public void ClickUpdate()
        {
            // Tìm phần tử
            var btnUpdate = Wait(By.XPath("//button[contains(.,'Cập nhật thông tin')]"));

            try
            {
                // 1. Cuộn trang đưa nút bấm vào vị trí GIỮA màn hình để tránh bị che bởi Header/Footer
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", btnUpdate);

                // Đợi 0.5s để hiệu ứng cuộn trang hoàn tất
                System.Threading.Thread.Sleep(500);

                // 2. Thử click theo cách thông thường
                btnUpdate.Click();
            }
            catch (ElementClickInterceptedException)
            {
                // 3. Nếu vẫn bị chặn (ví dụ do hiệu ứng overlay), ép click thẳng bằng lệnh JavaScript
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btnUpdate);
            }
        }

        public string GetSuccessMessage()
        {
            try
            {
                var msg = Wait(By.CssSelector(".bg-green-50"));
                return msg.Text;
            }
            catch
            {
                return "";
            }
        }
        public string GetHtml5ValidationMessage(string elementId)
        {
            try
            {
                var element = Wait(By.Id(elementId));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // Dùng JavaScript để lấy thông báo lỗi mặc định của HTML5
                string message = (string)js.ExecuteScript("return arguments[0].validationMessage;", element);

                return message; // Trả về chuỗi rỗng nếu không có lỗi, hoặc thông báo lỗi nếu có
            }
            catch
            {
                return "";
            }
        }
        public string GetSpanValidationError(string fieldName)
        {
            try
            {
                // Dùng FindElements để không bị chờ timeout lâu nếu phần tử không tồn tại
                var elements = driver.FindElements(By.CssSelector($"span[data-valmsg-for='{fieldName}']"));
                if (elements.Count > 0 && elements[0].Displayed)
                {
                    return elements[0].Text;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}