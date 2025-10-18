using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestProject2.TestHooks;

namespace TestProject2.Helper
{
    public class BasePage : DriverManager
    {

        #region Constants
        private const int DEFAULT_TIMEOUT_SECONDS = 20;
        private const int DEFAULT_POLLING_INTERVAL_SECONDS = 1;
        private const int DEFAULT_MAX_RETRIES = 3;
        private const int COOKIE_WAIT_SECONDS = 4;
        #endregion

        #region Properties
        protected IWebDriver Driver { get; }
        protected Actions Actions { get; }       
        protected WebDriverWait Wait { get; }
        protected string FileDownloadPath { get; }
        protected string TempFolderPath { get; }
        #endregion
        

        #region Constructor
        public BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Actions = new Actions(Driver);           
            Wait = CreateDefaultWait();
            TempFolderPath = Path.GetTempPath();
            FileDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }
        #endregion

        #region Wait Helpers
        private WebDriverWait CreateDefaultWait()
        {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(DEFAULT_TIMEOUT_SECONDS))
            {
                PollingInterval = TimeSpan.FromSeconds(DEFAULT_POLLING_INTERVAL_SECONDS)
            };
        }

        protected WebDriverWait CreateCustomWait(int timeoutSeconds)
        {
            return new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromSeconds(DEFAULT_POLLING_INTERVAL_SECONDS)
            };
        }

        /// <summary>
        /// Waits for the page to be fully loaded using document.readyState.
        /// </summary>
        protected void WaitForPageLoad()
        {
            Wait.Until(driver =>
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        /// <summary>
        /// Waits until a custom condition is met.
        /// </summary>
        protected void WaitUntil<T>(Func<T> function, T expected, int timeoutSeconds = 30)
        {
            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
            var actual = function();

            while (DateTime.Now < timeout && !Equals(actual, expected))
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                actual = function();
            }

            if (!Equals(actual, expected))
            {
                throw new TimeoutException($"Condition not met within {timeoutSeconds} seconds. Expected: {expected}, Actual: {actual}");
            }
        }
        #endregion

        #region Navigation
        /// <summary>
        /// Navigates to the specified URL and handles initial page setup.
        /// </summary>
        public void NavigateToUrl(string url)
        {
            Driver.Navigate().GoToUrl(url);           
            WaitForPageLoad();            
        }   

        public string GetCurrentUrl()
        {
            return Driver.Url;
        }
        #endregion

        #region Element Interaction
        /// <summary>
        /// Clicks on an element with retry logic for stale element exceptions.
        /// </summary>
        public void ClickElement(By locator)
        {
            RetryOnStaleElement(() =>
            {
                var element = Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
                element.Click();
            });
        }       

        /// <summary>
        /// Sends keys to an element after clearing existing content.
        /// </summary>
        public void SendKeys(By locator, string text)
        {
            RetryOnStaleElement(() =>
            {
                var element = Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
                element.Clear();
                element.SendKeys(text);
            });
        }

       
        #endregion

        #region Element Finding
        /// <summary>
        /// Finds a single element on the page.
        /// </summary>
        public IWebElement FindElement(By locator)
        {
            return Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
        }

        /// <summary>
        /// Finds multiple elements on the page.
        /// </summary>
        public IReadOnlyCollection<IWebElement> FindElements(By locator)
        {
            return Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.VisibilityOfAllElementsLocatedBy(locator));
        }

        /// <summary>
        /// Checks if an element is displayed on the page.
        /// </summary>
        public bool IsElementDisplayed(By locator)
        {
            try
            {
                var elements = Driver.FindElements(locator);
                return elements.Count > 0 && elements.First().Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        #endregion

        #region Element List Operations
        /// <summary>
        /// Clicks on an element at the specified index from a list of elements.
        /// </summary>
        public void ClickElementAtIndex(By locator, int index)
        {
            RetryOnStaleElement(() =>
            {
                var elements = FindElements(locator);
                if (index >= 0 && index < elements.Count)
                {
                    elements.ElementAt(index).Click();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Element count: {elements.Count}");
                }
            });
        }

        
        /// <summary>
        /// Gets text from an element at the specified index.
        /// </summary>
        public string GetTextFromElementAtIndex(By locator, int index)
        {
            return RetryOnStaleElement(() =>
            {
                var elements = FindElements(locator);
                if (index >= 0 && index < elements.Count)
                {
                    return elements.ElementAt(index).Text;
                }
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Element count: {elements.Count}");
            });
        }
        
        #endregion

        
        #region Utility Methods
        /// <summary>
        /// Retries an action if StaleElementReferenceException occurs.
        /// </summary>
        private void RetryOnStaleElement(Action action)
        {
            int retryCount = 0;

            while (retryCount < DEFAULT_MAX_RETRIES)
            {
                try
                {
                    action();
                    return;
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= DEFAULT_MAX_RETRIES)
                    {
                        throw;
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Retries a function if StaleElementReferenceException occurs.
        /// </summary>
        private T RetryOnStaleElement<T>(Func<T> function)
        {
            int retryCount = 0;

            while (retryCount < DEFAULT_MAX_RETRIES)
            {
                try
                {
                    return function();
                }
                catch (StaleElementReferenceException)
                {
                    retryCount++;
                    if (retryCount >= DEFAULT_MAX_RETRIES)
                    {
                        throw;
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException("Retry logic failed unexpectedly");
        }       
         

        #endregion
    }
}

