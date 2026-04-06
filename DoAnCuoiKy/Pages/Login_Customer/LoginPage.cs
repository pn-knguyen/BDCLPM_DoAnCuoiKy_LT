using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoAnCuoiKy.Pages.Login_Customer
{
    public class LoginPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public LoginPage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement ClickToLogin => wait.Until(d => d.FindElement(By.CssSelector("div[class='flex items-center space-x-2'] a[class='text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium transition-colors']")));
        private IWebElement Email => wait.Until(d => d.FindElement(By.CssSelector("#Email")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.CssSelector("#Password")));
        private IWebElement RememberMe => wait.Until(d => d.FindElement(By.CssSelector("#RememberMe")));
        private IWebElement LoginButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));

        private IWebElement WaitElement(By locator)
        {
            return wait.Until(d => d.FindElement(locator));
        }

        private void WaitForPageLoad()
        {
            try
            {
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString().Equals("complete", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
            }
        }

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
            WaitForPageLoad();
        }

        public void ClickToLoginPage()
        {
            ClickToLogin.Click();
            WaitForPageLoad();
        }

        public void EnterEmail(string email)
        {
            Email.Clear();
            Email.SendKeys(email);
        }

        public void EnterPassword(string password)
        {
            Password.Clear();
            Password.SendKeys(password);
        }

        public void ClickRememberMe()
        {
            SetRememberMe(true);
        }

        public void SetRememberMe(bool value)
        {
            var el = WaitElement(By.CssSelector("#RememberMe"));
            try
            {
                wait.Until(d => el.Displayed && el.Enabled);
                if (el.Selected != value)
                {
                    try
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                        Thread.Sleep(100);
                    }
                    catch { }

                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            el.Click();
                            Thread.Sleep(150);
                            if (el.Selected == value) return;
                        }
                        catch (OpenQA.Selenium.ElementClickInterceptedException)
                        {
                            try
                            {
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
                                Thread.Sleep(150);
                                if (el.Selected == value) return;
                            }
                            catch { }
                        }
                        catch (WebDriverException)
                        {
                            try
                            {
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
                                Thread.Sleep(150);
                                if (el.Selected == value) return;
                            }
                            catch { }
                        }
                        Thread.Sleep(100);
                    }

                    try
                    {
                        var label = driver.FindElement(By.CssSelector("label[for='RememberMe']"));
                        if (label.Displayed && label.Enabled)
                        {
                            label.Click();
                            Thread.Sleep(150);
                            if (el.Selected == value) return;
                        }
                    }
                    catch { }

                    try
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].checked = arguments[1]; arguments[0].dispatchEvent(new Event('change'));", el, value);
                        Thread.Sleep(100);
                    }
                    catch { }
                }
            }
            catch (WebDriverTimeoutException)
            {
                try
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("var el = document.querySelector('#RememberMe'); if(el){el.checked = arguments[0]; el.dispatchEvent(new Event('change'));}", value);
                    Thread.Sleep(100);
                }
                catch { }
            }
        }

        public void ClickLogin()
        {
            LoginButton.Click();
        }

        public string GetErrorMessage()
        {
            try
            {
                // wait until any of the possible error elements appears
                wait.Until(d =>
                {
                    if (d.FindElements(By.CssSelector(".validation-summary-errors ul li")).Count > 0) return true;
                    if (d.FindElements(By.CssSelector(".text-red-500.text-sm.field-validation-error")).Count > 0) return true;
                    if (d.FindElements(By.CssSelector(".text-red-500.text-xs.field-validation-error")).Count > 0) return true;
                    return false;
                });

                // try summary first
                var summary = driver.FindElements(By.CssSelector(".validation-summary-errors ul li"));
                if (summary.Count > 0) return summary[0].Text ?? "";

                // then small field-level
                var small = driver.FindElements(By.CssSelector(".text-red-500.text-sm.field-validation-error"));
                if (small.Count > 0) return small[0].Text ?? "";

                // then xs field-level; if multiple, join their text values
                var xs = driver.FindElements(By.CssSelector(".text-red-500.text-xs.field-validation-error"));
                if (xs.Count > 0)
                {
                    if (xs.Count == 1) return xs[0].Text ?? "";
                    var parts = new System.Collections.Generic.List<string>();
                    foreach (var e in xs)
                    {
                        var t = e.Text ?? "";
                        if (!string.IsNullOrWhiteSpace(t)) parts.Add(t.Trim());
                    }
                    return string.Join(" & ", parts);
                }

                return "";
            }
            catch (NoSuchElementException)
            {
                return "";
            }
            catch (WebDriverTimeoutException)
            {
                return "";
            }
            catch (WebDriverException)
            {
                return "";
            }
        }

        public string GetValidationMessage(string data)
        {
            if (string.IsNullOrEmpty(data)) return "";
            var cssSelector = data switch
            {
                "Email" => "#Email",
                "Password" => "#Password",
                _ => null
            };
            try
            {
                if (string.IsNullOrEmpty(cssSelector)) return "";
                var el = wait.Until(d => d.FindElement(By.CssSelector(cssSelector)));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                string errorMessage = (string)js.ExecuteScript("return arguments[0].validationMessage;", el);
                return errorMessage ?? "";
            }
            catch (NoSuchElementException)
            {
                return "";
            }
            catch (WebDriverTimeoutException)
            {
                return "";
            }
            catch (WebDriverException)
            {
                return "";
            }
        }
    }
}