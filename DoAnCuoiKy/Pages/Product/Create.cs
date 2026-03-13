using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages
{
    public class CreatePage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public CreatePage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement Wait(By locator)
        {
            return wait.Until(d =>
            {
                var el = d.FindElement(locator);
                return el.Displayed ? el : null;
            });
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
            Wait(By.CssSelector("button[type='submit']")).Click();
        }

        public void OpenProductManagement()
        {
            Wait(By.LinkText("Quản lý sản phẩm")).Click();
        }

        public void ClickCreateProduct()
        {
            Wait(By.LinkText("Thêm sản phẩm mới")).Click();
        }

        public void EnterProductName(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var productName = Wait(By.Id("ProductName"));
                productName.Clear();
                productName.SendKeys(data);
            }
        }

        public void SelectCategory(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var category = new SelectElement(Wait(By.Id("CategoryId")));
                category.SelectByText(data);
            }
        }

        public void EnterPrice(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var price = Wait(By.Id("Price"));
                price.Clear();
                price.SendKeys(data);
            }
        }

        public void EnterStock(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var stock = Wait(By.Id("Stock"));
                stock.Clear();
                stock.SendKeys(data);
            }
        }

        public void SelectDiscount(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var discount = new SelectElement(Wait(By.Id("DiscountId")));
                discount.SelectByText(data);
            }
        }

        public void EnterShortDescription(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var shortDesc = Wait(By.Id("ShortDescription"));
                shortDesc.Clear();
                shortDesc.SendKeys(data);
            }
        }

        public void EnterDetailDescription(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var detailDesc = Wait(By.Id("DetailDescription"));
                detailDesc.Clear();
                detailDesc.SendKeys(data);
            }
        }

        public void SelectColor(string data, int index)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var colors = wait.Until(d => d.FindElements(By.Name("Colors")));
                var colorDropdown = new SelectElement(colors[index]);
                colorDropdown.SelectByText(data);
            }
        }

        public void UploadImage(string path, int index)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                var images = wait.Until(d => d.FindElements(By.Name("Images")));
                images[index].SendKeys(path);
            }
        }

        public void Submit()
        {
            Wait(By.Id("submitButton")).Click();
        }
        public void AddColor()
        {
            Wait(By.CssSelector(".mt-4")).Click();
        }

        public string GetSuccessMessage()
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".sm\\:inline"));
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
                var errors = driver.FindElements(By.CssSelector(".text-red-500, .validation-error, .text-danger, span.field-validation-error, .bg-red-100.border.border-red-400.text-red-700.px-4.py-3.rounded.relative.mb-4, .h2"));

                var result = string.Join(", ", errors.Select(e => e.Text).Where(t => !string.IsNullOrWhiteSpace(t)));

                return result;
            }
            catch
            {
                return "";
            }
        }
    }
}
