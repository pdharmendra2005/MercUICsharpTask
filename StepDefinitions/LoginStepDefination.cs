using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject2.Pages;

namespace TestProject2.StepDefinitions
{
    [Binding]
    public class LoginStepDefination : LoginPage
    {
        private readonly ScenarioContext _scenarioContext;
        private IConfiguration config;

        public LoginStepDefination(IWebDriver driver, ScenarioContext scenarioContext) : base(driver) 
        {
            config = scenarioContext.Get<IConfiguration>("config");
            _scenarioContext = scenarioContext;
        }

        [Given("I am on the login page")]
        public void GivenIAmOnTheLoginPage()
        {
            GoToLoginPage(config["LoginPageUrl"]);
        }

        [When("I login with username {string} and password {string}")]
        public void WhenILoginWithUsernameAndPassword(string username, string password)
        {
            EnterUserName(username);
            EnterPassword(password);
            ClickLoginButton();
        }
    }   

}
