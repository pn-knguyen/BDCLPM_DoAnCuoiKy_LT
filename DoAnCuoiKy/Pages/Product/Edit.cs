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
            if (colors.Count == 0 || string.IsNullOrWhiteSpace(color)) return;

            var targetIndex = Math.Min(Math.Max(index, 0), colors.Count - 1);
            var dropdown = new SelectElement(colors[targetIndex]);
            var desiredColor = color.Trim();

            var selectedColors = colors
                .Where((_, i) => i != targetIndex)
                .Select(c => new SelectElement(c).SelectedOption.Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (selectedColors.Contains(desiredColor))
            {
                var fallback = dropdown.Options
                    .Select(o => o.Text.Trim())
                    .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o) && !selectedColors.Contains(o));

                if (!string.IsNullOrWhiteSpace(fallback))
                    desiredColor = fallback;
            }

            dropdown.SelectByText(desiredColor);
        }

        public void AddColor()
        {
            Wait(By.Id("addColorBtn")).Click();
        }

        public void RemoveColor()
        {
            var removeColorButtons = wait.Until(d => d.FindElements(By.CssSelector(".color-section:nth-child(2) .px-3:nth-child(2)")));
            if (removeColorButtons.Count == 0) return;

            var btn = removeColorButtons[0];

            try
            {
                btn.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            }

            TryAcceptAlert();
        }

        public int GetColorCount()
        {
            try
            {
                return driver.FindElements(By.Name("Colors")).Count;
            }
            catch
            {
                return 0;
            }
        }

        // ===== Upload image =====

        public void UploadNewImage(string path)
        {
            UploadNewImage(path, 0);
        }

        public void UploadNewImage(string path, int index)
        {
            var normalizedPath = (path ?? string.Empty)
                .Trim()
                .Trim('"', '“', '”');

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return;

            if (!Path.IsPathRooted(normalizedPath))
                normalizedPath = Path.GetFullPath(normalizedPath);

            if (!File.Exists(normalizedPath))
                return;

            var imageInputs = wait.Until(d => d.FindElements(By.CssSelector(".image-input")));
            if (imageInputs.Count == 0) return;

            var targetIndex = Math.Min(Math.Max(index, 0), imageInputs.Count - 1);
            imageInputs[targetIndex].SendKeys(normalizedPath);
        }

        //remove image
        public void RemoveImage(int index)
        {
            var removeBtns = wait.Until(d => d.FindElements(By.CssSelector(".relative .w-4")));
            if (removeBtns.Count == 0) return;

            var btn = removeBtns[Math.Min(index, removeBtns.Count - 1)];

            try
            {
                btn.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
            }
        }

        // ===== Submit / Cancel =====

        public void SubmitEdit()
        {
            Wait(By.Id("submitBtn")).Click();
            TryAcceptAlert();
        }

        private void TryAcceptAlert()
        {
            try
            {
                var shortWait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
                shortWait.Until(d =>
                {
                    try
                    {
                        var alert = d.SwitchTo().Alert();
                        alert.Accept();
                        return true;
                    }
                    catch (NoAlertPresentException)
                    {
                        return false;
                    }
                });
            }
            catch
            {
            }
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
                var errors = driver.FindElements(By.CssSelector("#Price-error, #Stock-error"));

                var result = string.Join(", ", errors.Select(e => e.Text).Where(t => !string.IsNullOrWhiteSpace(t)));

                return result;
            }
            catch
            {
                return "";
            }
        }

        public bool IsSubmitDisabled()
        {
            try
            {
                var submit = Wait(By.Id("submitBtn"));
                var disabledAttr = submit.GetAttribute("disabled");
                var ariaDisabled = submit.GetAttribute("aria-disabled");
                var classes = submit.GetAttribute("class") ?? "";

                return !string.IsNullOrEmpty(disabledAttr)
                       || string.Equals(ariaDisabled, "true", StringComparison.OrdinalIgnoreCase)
                       || classes.Contains("disabled", StringComparison.OrdinalIgnoreCase)
                       || !submit.Enabled;
            }
            catch
            {
                return false;
            }
        }

        public int GetRemoveImageButtonCount()
        {
            try
            {
                return driver.FindElements(By.CssSelector(".relative .w-4")).Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}