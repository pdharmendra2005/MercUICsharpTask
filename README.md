# UI Test Automation Framework

Automated UI testing framework using BDD (Behavior-Driven Development) approach with Selenium WebDriver.

## 📋 Overview

This framework provides automated UI testing capabilities with:
- Multi-browser support (Chrome, Firefox, Edge)
- Page Object Model design pattern
- ExtentReports with screenshots
- Thread-safe parallel execution
- Environment-based configuration

## 🛠️ Tech Stack

- **Language:** C# (.NET 6.0+)
- **BDD Framework:** Reqnroll (Gherkin/SpecFlow successor)
- **Browser Automation:** Selenium WebDriver 4.x
- **Test Runner:** NUnit 3.x
- **Reporting:** ExtentReports 5.x
- **Configuration:** Microsoft.Extensions.Configuration
- **Wait Helpers:** SeleniumExtras.WaitHelpers

## 🚀 Quick Start

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download) or higher
- Visual Studio 2022 / VS Code / Rider
- Chrome, Firefox, or Edge browser

### Installation
```bash
# Clone repository
git clone <repository-url>
cd TestProject2

# Restore packages
dotnet restore

# Build project
dotnet build

# Run all tests
dotnet test
```

## ⚙️ Configuration

Edit `appSettings.json`:
```json
{
  "BrowserChoice": "chrome"
  
}
```

**Supported Browsers:** `chrome` | `firefox` | `edge` | `msedge`



### Run with tags/categories
```bash
dotnet test --filter "Category=Inventory"
```


## 📊 Reports

HTML reports with screenshots are automatically generated after each test run.

**Location:** `Screenshots/index.html`

**View report:**
```bash
# Windows
start Screenshots/index.html

# Mac
open Screenshots/index.html

# Linux
xdg-open Screenshots/index.html
```

**Report includes:**
- ✅ Pass/fail statistics
- 📸 Screenshots on failure
- 🕐 Execution timeline
- 📝 Step-by-step details


