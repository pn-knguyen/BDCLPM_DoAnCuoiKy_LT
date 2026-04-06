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
                var errorElement = wait.Until(d =>
                {
                    // validation summary
                    var els = d.FindElements(By.CssSelector(".validation-summary-errors ul li"));
                    if (els.Count > 0) return els[0];

                    // field level validation variants
                    var elsSm = d.FindElements(By.CssSelector(".text-red-500.text-sm.field-validation-error"));
                    if (elsSm.Count > 0) return elsSm[0];

                    var elsXs = d.FindElements(By.CssSelector(".text-red-500.text-xs.field-validation-error"));
                    if (elsXs.Count == 1) return elsXs[0];
                    else return $"{elsXs[0]} & {elsXs[1]}";

                    return null;
                });

                return errorElement?.Text ?? "";
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