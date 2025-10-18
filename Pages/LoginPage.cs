using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject2.Helper;

namespace TestProject2.Pages
{
    public class LoginPage : BasePage
    {
        #region Elements
        private By Username => By.Id("user-name");
        private By Password => By.Id("password");
        private By LoginButton => By.Id("login-button");

        #endregion

        public LoginPage(IWebDriver driver) : base(driver)
        {
        }

        #region Functions

        public void GoToLoginPage(string pageUrl)
        {
            NavigateToUrl(pageUrl);
        }

        public void EnterUserName(string userName)
        {
            SendKeys(Username, userName);

        }

        public void EnterPassword(string password)
        {
            SendKeys(Password, password);
        }

        public void ClickLoginButton() 
        {
            ClickElement(LoginButton);
        }

        #endregion

    }
}
