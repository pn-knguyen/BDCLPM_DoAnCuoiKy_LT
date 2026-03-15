using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy.Pages.Login
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

        private IWebElement Username => wait.Until(d => d.FindElement(By.Id("Username")));
        private IWebElement Password => wait.Until(d => d.FindElement(By.Id("Password")));
        private IWebElement LoginButton => wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));

        public void Navigate(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public void EnterUsername(string username)
        {
            Username.Clear();
            Username.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            Password.Clear();
            Password.SendKeys(password);
        }

        public void ClickLogin()
        {
            LoginButton.Click();
        }

        public string GetErrorMessage()
        {
            try
            {
                var error = wait.Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(By.CssSelector(".font-medium"));
                        return el.Displayed ? el : null;
                    }
                    catch
                    {
                        return null;
                    }
                });

                return error?.Text ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}