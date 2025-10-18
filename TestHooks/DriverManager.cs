using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject2.TestHooks
{
    public class DriverManager
    {
        protected IWebDriver Driver { get; set; }

        #region Launch Browser
        public IWebDriver LaunchBrowser(string browser)
        {
            browser = browser?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(browser));

            switch (browser)
            {
                case "chrome":
                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--incognito");
                    chromeOptions.AddArguments("--disable-infobars");
                    chromeOptions.AddArguments("--no-sandbox");
                    chromeOptions.AddArguments("--disable-gpu");
                    chromeOptions.AddArguments("--disable-setuid-sandbox");
                    chromeOptions.AddArgument("--window-size=1936,1056");
                    chromeOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                    chromeOptions.AddArgument("--headless");
                    Driver = new ChromeDriver(chromeOptions);
                    break;

                case "firefox":
                    FirefoxOptions firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArguments("--disable-infobars");
                    firefoxOptions.AddArguments("-private");
                    Driver = new FirefoxDriver(firefoxOptions);
                    Driver.Manage().Window.Maximize();
                    break;

                case "edge":
                case "msedge":
                    EdgeOptions edgeOptions = new EdgeOptions();
                    edgeOptions.AddArguments("--disable-infobars");
                    edgeOptions.AddArgument("--incognito");
                    edgeOptions.AddArguments("--disable-gpu");
                    edgeOptions.PageLoadStrategy = PageLoadStrategy.Normal;
                    Driver = new EdgeDriver(edgeOptions);
                    Driver.Manage().Window.Maximize();
                    break;

                default:
                    throw new ArgumentException($"Browser type '{browser}' is not supported. Supported browsers: chrome, firefox, edge");
            }

            Debug.Assert(Driver != null, nameof(Driver) + " != null");
            return Driver;
        }

        #endregion

        #region Close Browser
        public void CloseBrowser()
        {
            try
            {
                Driver?.Quit();
                Driver = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Driver failed to quit: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Screenshot Management
        public static void CleanScreenShotFolder()
        {
            var screenShotFolder = GetScreenShotFolderPath();
            if (!Directory.Exists(screenShotFolder))
            {
                Directory.CreateDirectory(screenShotFolder);
            }

            var filePaths = Directory.GetFiles(screenShotFolder);
            foreach (var filePath in filePaths)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file {filePath}: {ex.Message}");
                }
            }
        }

        public static string GetScreenShotFolderPath()
        {
            var path = Assembly.GetCallingAssembly().Location;
            var actualPath = path.Substring(0, path.LastIndexOf("bin", StringComparison.Ordinal));
            var projectPath = new Uri(actualPath).LocalPath;
            var screenShotFolder = Path.Combine(projectPath, "Screenshots");
            return screenShotFolder;
        }

        public void TakeScreenShot()
        {
            if (Driver == null)
            {
                Console.WriteLine("Cannot take screenshot: Driver is null");
                return;
            }

            var time = DateTime.Now;
            var dateTimeNow = $"_{time:dd-MM-yyyy}_{time:HH-mm-ss}_";
            var nameOfTest = TestContext.CurrentContext.Test.MethodName;

            try
            {
                var screenShot = ((ITakesScreenshot)Driver).GetScreenshot();
                var filePath = Path.Combine(GetScreenShotFolderPath(), $"{nameOfTest}{dateTimeNow}screenshot.png");
                screenShot.SaveAsFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screenshot capture failed: {ex.Message}");
            }
        }
        #endregion

        #region Process Management
        public static void KillBrowserProcesses(string browserProcessName)
        {
            try
            {
                var processes = Process.GetProcessesByName(browserProcessName);
                foreach (var proc in processes)
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                            proc.WaitForExit(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to kill process {proc.ProcessName} (ID: {proc.Id}): {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing browser processes: {ex.Message}");
            }
        }
        #endregion
    }

}

