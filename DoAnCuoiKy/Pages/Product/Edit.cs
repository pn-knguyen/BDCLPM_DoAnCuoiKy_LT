using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages
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

        public void ClickEditProduct()
        {
            Wait(By.LinkText("Sửa")).Click();
        }

        // ===== Product basic info =====

        public void EnterProductName(string data)
        {
            var productName = Wait(By.Id("ProductName"));
            productName.Clear();
            productName.SendKeys(data);
        }

        public void SelectCategory(string data)
        {
            var category = new SelectElement(Wait(By.Id("CategoryId")));
            category.SelectByText(data);
        }

        public void EnterPrice(string data)
        {
            var price = Wait(By.Id("Price"));
            price.Clear();
            price.SendKeys(data);
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
            var discount = new SelectElement(Wait(By.Id("DiscountId")));
            discount.SelectByText(data);
        }

        public void EnterShortDescription(string data)
        {
            var shortDesc = Wait(By.Id("ShortDescription"));
            shortDesc.Clear();
            shortDesc.SendKeys(data);
        }

        public void EnterDetailDescription(string data)
        {
            var detailDesc = Wait(By.Id("DetailDescription"));
            detailDesc.Clear();
            detailDesc.SendKeys(data);
        }

        // ===== Color section =====

        public void SelectColor(string color, int index)
        {
            var colors = wait.Until(d => d.FindElements(By.Name("Colors")));
            var dropdown = new SelectElement(colors[index]);
            dropdown.SelectByText(color);
        }

        public void AddColor()
        {
            Wait(By.Id("addColorBtn")).Click();
        }

        public void RemoveColor(int index)
        {
            var removeBtns = wait.Until(d => d.FindElements(By.CssSelector(".color-section .w-5")));
            removeBtns[index].Click();
        }

        // ===== Upload image =====

        public void UploadNewImage(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                var image = Wait(By.CssSelector(".image-input"));
                image.SendKeys(path);
            }
        }

        // ===== Submit / Cancel =====

        public void SubmitEdit()
        {
            Wait(By.Id("submitBtn")).Click();
        }

        public void CancelEdit()
        {
            Wait(By.LinkText("Hủy")).Click();
        }
        public string GetSuccessMessage()
        {
            try
            {
                var msg = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".sm\\3Ainline"));
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
                var errors = driver.FindElements(By.CssSelector("#Price-error"));

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