using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Reqnroll.BoDi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject2.TestHooks
{
    [Binding]
    public class Hooks : DriverManager
    {
        private readonly IObjectContainer _container;
        private readonly ScenarioContext _scenarioContext;
        private static ExtentReports _extent;
        private static readonly ThreadLocal<ExtentTest> _featureName = new ThreadLocal<ExtentTest>();
        private static readonly ThreadLocal<ExtentTest> _extentScenario = new ThreadLocal<ExtentTest>();
        private static IConfiguration _configuration;

        public Hooks(IObjectContainer container, ScenarioContext scenarioContext)
        {
            _container = container;
            _scenarioContext = scenarioContext;
        }

        #region Test Run Hooks
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            InitializeConfiguration();
            ValidateConfiguration();
            InitializeReporting();
            CleanScreenShotFolder();
            KillExistingBrowserProcesses();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _extent?.Flush();
            KillExistingBrowserProcesses();
        }
        #endregion

        #region Feature Hooks
        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            _featureName.Value = _extent.CreateTest<Feature>(featureContext.FeatureInfo.Title);
        }
        #endregion

        #region Scenario Hooks
        [BeforeScenario(Order = -1)]
        public void BeforeScenario()
        {
            _scenarioContext.Add("config", _configuration);

            string browserChoice = GetConfigValue("BrowserChoice");
            Driver = LaunchBrowser(browserChoice);
            _container.RegisterInstanceAs(Driver);
            _scenarioContext.Add("WebDriver", Driver);

            _extentScenario.Value = _featureName.Value.CreateNode<Scenario>(_scenarioContext.ScenarioInfo.Title);
        }

        [AfterScenario]
        public void AfterScenario()
        { 
            TakeScreenShot();
            CloseBrowser();
        }
        #endregion

        #region Step Hooks
        [AfterStep]
        public void InsertReportingSteps()
        {
            var stepContext = _scenarioContext.StepContext;
            var stepType = stepContext.StepInfo.StepDefinitionType.ToString();
            var stepText = stepContext.StepInfo.Text;

            if (_scenarioContext.TestError == null)
            {
                CreatePassedStepNode(stepType, stepText);
            }
            else
            {
                CreateFailedStepNode(stepType, stepText, _scenarioContext.TestError.Message);
            }
        }
        #endregion

        #region Configuration
        private static void InitializeConfiguration()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var assemblyConfigurationAttribute = typeof(Hooks).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{buildConfigurationName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void ValidateConfiguration()
        {
            var requiredKeys = new[] { "BrowserChoice" };
            foreach (var key in requiredKeys)
            {
                if (string.IsNullOrEmpty(_configuration[key]))
                {
                    throw new InvalidOperationException($"Missing required configuration key: {key}");
                }
            }
        }

        private static string GetConfigValue(string key)
        {
            string value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration key '{key}' not found in config or environment variables");
            }

            return value;
        }
        #endregion

        #region Reporting
        private static void InitializeReporting()
        {
            string reportName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string reportPath = GetScreenShotFolderPath();
            string reportFile = Path.Combine(reportPath, "index.html");

            var htmlReporter = new ExtentSparkReporter(reportFile);
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;
            htmlReporter.Config.ReportName = reportName;
            htmlReporter.Config.DocumentTitle = "Test Automation Report";

            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
        }

        private void CreatePassedStepNode(string stepType, string stepText)
        {
            switch (stepType)
            {
                case "Given":
                    _extentScenario.Value.CreateNode<Given>(stepText);
                    break;
                case "When":
                    _extentScenario.Value.CreateNode<When>(stepText);
                    break;
                case "Then":
                    _extentScenario.Value.CreateNode<Then>(stepText);
                    break;
                case "And":
                    _extentScenario.Value.CreateNode<And>(stepText);
                    break;
            }
        }

        private void CreateFailedStepNode(string stepType, string stepText, string errorMessage)
        {
            switch (stepType)
            {
                case "Given":
                    _extentScenario.Value.CreateNode<Given>(stepText).Fail(errorMessage);
                    break;
                case "When":
                    _extentScenario.Value.CreateNode<When>(stepText).Fail(errorMessage);
                    break;
                case "Then":
                    _extentScenario.Value.CreateNode<Then>(stepText).Fail(errorMessage);
                    break;
                case "And":
                    _extentScenario.Value.CreateNode<And>(stepText).Fail(errorMessage);
                    break;
            }
        }
        #endregion

        #region Browser Management
        private static void KillExistingBrowserProcesses()
        {
            string browserType = GetConfigValue("BrowserChoice").ToLowerInvariant();

            switch (browserType)
            {
                case "edge":
                case "msedge":
                    KillBrowserProcesses("msedge");
                    break;
                case "chrome":
                    KillBrowserProcesses("chrome");
                    break;
                case "firefox":
                    KillBrowserProcesses("firefox");
                    break;
                default:
                    throw new ArgumentException($"Browser type '{browserType}' is not supported for process cleanup");
            }
        }

        #endregion

    }
}
