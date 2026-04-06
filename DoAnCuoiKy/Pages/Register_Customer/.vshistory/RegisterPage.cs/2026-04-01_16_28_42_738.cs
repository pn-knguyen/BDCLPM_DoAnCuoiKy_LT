using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Threading;

namespace DoAnCuoiKy.Pages.Register_Customer
{
    public class RegisterPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RegisterPage(IWebDriver driver, WebDriverWait wait)
        {
            this.driver = driver;
            this.wait = wait;
        }

        private IWebElement ClickToRegister => wait.Until(d => d.FindElement(By.CssSelector("a[class='bg-primary-600 text-white hover:bg-primary-700 px-4 py-2 rounded-md text-sm font-medium transition-colors']")));
        private IWebElement FirstName => wait.Until(d => d.FindElement(By.CssSelector("#FirstName")));
        private IWebElement LastName => wait.Until(d => d.FindElement(By.CssSelector("#LastName")));
        private IWebElement Email => wait.Until(d => d.FindElement(By.CssSelector("#Email")));
        private IWebElement PhoneNumber => wait.Until(d => d.FindElement(By.CssSelector("#Phone")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.CssSelector("#Password")));
        private IWebElement VerifyPassword => wait.Until(d => d.FindElement(By.CssSelector("#ConfirmPassword")));
        private IWebElement AgreeTerms => wait.Until(d => d.FindElement(By.CssSelector("#agree-terms")));
        private IWebElement RegisterButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));

        private IWebElement WaitElement(By locator)
        {
            return wait.Until(d => d.FindElement(locator));
        }

        private bool IsElementTopMost(IWebElement el)
        {
            try
            {
                var js = (IJavaScriptExecutor)driver;
                // compute center and get elementFromPoint
                var script = @"
                    var el = arguments[0];
                    var rect = el.getBoundingClientRect();
                    var x = rect.left + rect.width/2;
                    var y = rect.top + rect.height/2;
                    return document.elementFromPoint(x,y) === el || el.contains(document.elementFromPoint(x,y));
                ";
                var result = js.ExecuteScript(script, el);
                return result is bool b && b;
            }
            catch
            {
                return false;
            }
        }

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void ClickToRegisterPage()
        {
            ClickToRegister.Click();
        }

        public void EnterFirstName(string firstName)
        {
            FirstName.Clear();
            FirstName.SendKeys(firstName);
        }

        public void EnterLastName(string lastName)
        {
            LastName.Clear();
            LastName.SendKeys(lastName);
        }

        public void EnterEmail(string email)
        {
            Email.Clear();
            Email.SendKeys(email);
        }

        public void EnterPhoneNumber(string phoneNumber)
        {
            PhoneNumber.Clear();
            PhoneNumber.SendKeys(phoneNumber);
        }

        public void EnterPassword(string password)
        {
            Password.Clear();
            Password.SendKeys(password);
        }

        public void EnterVerifyPassword(string verifyPassword)
        {
            VerifyPassword.Clear();
            VerifyPassword.SendKeys(verifyPassword);
        }

        public void ClickAgreeTerms()
        {
            SetAgreeTerms(true);
        }

        public void SetAgreeTerms(bool value)
        {
            var el = WaitElement(By.CssSelector("#agree-terms"));
            try
            {
                wait.Until(d => el.Displayed && el.Enabled);

                if (el.Selected != value)
                {
                    // try to scroll into view first
                    try
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                        Thread.Sleep(100);
                    }
                    catch { }

                    // try clicking several times, handle interception
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

                    // try clicking the label
                    try
                    {
                        var label = driver.FindElement(By.CssSelector("label[for='agree-terms']"));
                        if (label.Displayed && label.Enabled)
                        {
                            label.Click();
                            Thread.Sleep(150);
                            if (el.Selected == value) return;
                        }
                    }
                    catch { }

                    // final fallback: set checked via JS and dispatch change
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
                    ((IJavaScriptExecutor)driver).ExecuteScript("var el = document.querySelector('#agree-terms'); if(el){el.checked = arguments[0]; el.dispatchEvent(new Event('change'));}", value);
                    Thread.Sleep(100);
                }
                catch { }
            }
        }

        public bool IsAgreeTermsChecked()
        {
            try
            {
                var el = WaitElement(By.CssSelector("#agree-terms"));
                return el.Selected;
            }
            catch { return false; }
        }

        public void DoNotThing()
        {
        }

        public void Submit()
        {
            RegisterButton.Click();
        }

        public string GetErrorMessage()
        {
            try
            {
                var errorElement = wait.Until(d => d.FindElement(By.CssSelector(".text-red-500.text-sm.field-validation-error")));
                return errorElement.Text ?? "";
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

        public string GetValidationErrors(string data)
        {
            if (string.IsNullOrEmpty(data))
                return "";
            var cssSelector = data switch
            {
                "FirstName" => "#FirstName",
                "LastName" => "#LastName",
                "Email" => "#Email",
                "Phone" => "#Phone",
                "Password" => "#Password",
                "ConfirmPassword" => "#ConfirmPassword",
                "Terms" => "#agree-terms",
                _ => null
            };
            try
            {
                if (string.IsNullOrEmpty(cssSelector))
                    return "";

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

        public string GetSuccessMessage()
        {
            var url = driver.Url;
            if (url == "http://localhost:5167/")
            {
                return "Đăng ký thành công";
            }
            else
            {
                return "";
            }
        }
    }
}