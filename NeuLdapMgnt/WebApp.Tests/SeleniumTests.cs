using NeuLdapMgnt.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace NeuLdapMgnt.WebApp.Tests;

[TestClass]
public class SeleniumTests
{
    private static readonly string EnvironmentMode =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

    private static readonly string Username =
        Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME") ?? "admin";

    private static readonly string Password =
        Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? "adminpass";

    private static readonly string SutHub = 
        Environment.GetEnvironmentVariable("SELENIUM_HUB_URL") ?? "http://selenium-hub:4444";
    
    private static readonly string SutMngtDocker =  
        Environment.GetEnvironmentVariable("WEBAPP_URL") ?? "http://selenium-nginx:80";

    private const string SutMngtLocal = "http://localhost:8080";
    private static string _sutMngt = string.Empty;
    private static IWebDriver _webDriver = default!;
    private static WebDriverWait _wait = default!;

    private static List<IWebElement> NavLinks
        => _webDriver.FindElements(By.ClassName("nav-link")).ToList();

    [ClassInitialize]
    public static void InitializeClass(TestContext context)
    {
        _sutMngt = !EnvironmentMode.Equals("Docker") ? SutMngtLocal : SutMngtDocker;
        var firefoxOptions = new FirefoxOptions();
        if (!EnvironmentMode.Equals("Docker"))
        {
            _webDriver = new FirefoxDriver(firefoxOptions);
        }
        else
        {
            _webDriver = new RemoteWebDriver(new Uri(SutHub), firefoxOptions.ToCapabilities());
        }

        _wait = new(_webDriver, TimeSpan.FromMilliseconds(5000));
    }

    [ClassCleanup]
    public static void CleanupClass()
    {
        _webDriver.Quit();
    }

    [TestInitialize]
    public void InitializeTest()
    {
        _webDriver.Navigate().GoToUrl(_sutMngt);
    }

    private static void Login()
    {
        _wait.Until(ExpectedConditions.UrlContains("/login"));
        _webDriver.FindElement(By.Id("username")).SendKeys(Username);
        _webDriver.FindElement(By.Id("password")).SendKeys(Password);
        _webDriver.FindElement(By.ClassName("btn-outline-primary")).Submit();
        _wait.Until(ExpectedConditions.TitleIs("Home"));
    }

    private static void ExpandNavbar()
    {
        var navItems = _webDriver
            .FindElements(By.ClassName("nav-item"))
            .Where(x => !x.Text.Contains("Logout") && !x.Text.Contains("Home"))
            .ToList();
        navItems.ForEach(x => x.Click());
    }

    [TestMethod]
    public void DefaultRedirectionToLoginPage()
    {
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);
    }

    [TestMethod]
    public void RedirectionIsWorkingWhenUnauthorized()
    {
        _webDriver.Navigate().GoToUrl(_sutMngt + "/students");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);

        _webDriver.Navigate().GoToUrl(_sutMngt + "/students/add");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);

        _webDriver.Navigate().GoToUrl(_sutMngt + "/teachers");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);

        _webDriver.Navigate().GoToUrl(_sutMngt + "/teachers/add");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);

        _webDriver.Navigate().GoToUrl(_sutMngt + "/db/admins");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);
    }

    [TestMethod]
    public void SuccessfulLoginRedirectsToHomePage()
    {
        Login();
        Assert.AreEqual("Home", _webDriver.Title);
    }

    [TestMethod]
    public void NavbarLinksAreWorking()
    {
        Login();
        ExpandNavbar();

        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Students"));
        Assert.AreEqual("Students", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Add Student"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Student"));
        Assert.AreEqual("Add Student", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Teachers"));
        Assert.AreEqual("Teachers", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Add Teacher"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Teacher"));
        Assert.AreEqual("Add Teacher", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Admins"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Admins"));
        Assert.AreEqual("Admins", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Inactive Users"));
        Assert.AreEqual("Inactive Users", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Classes"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Classes"));
        Assert.AreEqual("Classes", _webDriver.Title);
        
        NavLinks.Find(x => x.Text.Equals("Request Logs"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Request Logs"));
        Assert.AreEqual("Request Logs", _webDriver.Title);
        
        NavLinks.Find(x => x.Text.Equals("Import/Export"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Database Import/Export"));
        Assert.AreEqual("Database Import/Export", _webDriver.Title);
        
        NavLinks.Find(x => x.Text.Equals("Default Admin"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Default Admin Settings"));
        Assert.AreEqual("Default Admin Settings", _webDriver.Title);
        
        NavLinks.Find(x => x.Text.Equals("Home"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Home"));
        Assert.AreEqual("Home", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Logout"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);
    }

    [TestMethod]
    public void NoStudentsArePresent()
    {
        Login();
        ExpandNavbar();

        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Students"));
        _wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.TagName("h3")),
            "There are no [Active] students"));

        Assert.AreEqual("There are no [Active] students", _webDriver.FindElement(By.TagName("h3")).Text);
    }

    [TestMethod]
    public void NoStudentsArePresentAndAddStudentsButtonIsPresent()
    {
        Login();
        ExpandNavbar();

        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
            "Add Students"));

        Assert.IsTrue(_webDriver.FindElement(By.ClassName("btn-primary")).Text.Contains("Add Students"));
    }

    [TestMethod]
    public void AfterPressingAddStudentsButtonRedirectsToAddStudent()
    {
        Login();
        ExpandNavbar();

        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
            "Add Students"));

        _webDriver.FindElement(By.ClassName("btn-primary")).Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Student"));
        Assert.AreEqual("Add Student", _webDriver.Title);
    }

    [TestMethod]
    public void AddStudentsEditFormHasLoadedDefaultValues()
    {
        Login();
        ExpandNavbar();

        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),"Add Students"));
        _webDriver.FindElement(By.ClassName("btn-primary")).Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Student"));

        var form = _webDriver.FindElement(By.TagName("form"));
        Assert.AreEqual(Student.IdMinValue.ToString(),
            form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).GetAttribute("value"));

        Assert.AreEqual(string.Empty, form.FindElement(By.Id("first-name")).GetAttribute("value"));
        Assert.AreEqual(string.Empty, form.FindElement(By.Id("last-name")).GetAttribute("value"));
        Assert.AreEqual(string.Empty, form.FindElement(By.Id("middle-name")).GetAttribute("value"));
        Assert.AreEqual(string.Empty, form.FindElement(By.Id("class-select")).GetAttribute("value"));
        Assert.AreEqual(string.Empty, form.FindElement(By.Id("password")).GetAttribute("value"));
    }

    [TestMethod]
    public void AddStudentsEditFormIsValidatingOmCorrectly()
    {
        Login();
        ExpandNavbar();
        NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
        _wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
            "Add Students"));

        _webDriver.FindElement(By.ClassName("btn-primary")).Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Student"));

        var form = _webDriver.FindElement(By.TagName("form"));
        form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.ArrowDown);
        Assert.AreEqual($"OM must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
            form.FindElement(By.Id("student-om-validation-message")).Text);

        for (int i = 0; i < 11; i++)
        {
            form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.Backspace);
        }

        form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys("80000000000");
        Assert.AreEqual($"OM must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
            form.FindElement(By.Id("student-om-validation-message")).Text);
    }

    [TestMethod]
    public void AddStudentsEditFormIsValidatingFullNameCorrectly()
    {
        Login();
        ExpandNavbar();
        NavLinks.Find(x => x.Text.Equals("Add Student"))?.Click();

        var form = _webDriver.FindElement(By.TagName("form"));
        form.FindElement(By.Id("first-name")).SendKeys(Keys.Space);
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.AreEqual("First name is required.",
            form.FindElement(By.Id("student-first-name-validation-message")).Text);

        form.FindElement(By.Id("first-name")).SendKeys("Aa");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.AreEqual("First name must be at least 3 characters long.",
            form.FindElement(By.Id("student-first-name-validation-message")).Text);

        form.FindElement(By.Id("first-name")).SendKeys("Test");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.ThrowsException<NoSuchElementException>(() =>
            form.FindElement(By.Id("student-first-name-validation-message")));

        form.FindElement(By.Id("last-name")).SendKeys(Keys.Space);
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.AreEqual("Last name is required.",
            form.FindElement(By.Id("student-last-name-validation-message")).Text);

        form.FindElement(By.Id("last-name")).SendKeys("Aa");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.AreEqual("Last name must be at least 3 characters long.",
            form.FindElement(By.Id("student-last-name-validation-message")).Text);

        form.FindElement(By.Id("last-name")).SendKeys("Test");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.ThrowsException<NoSuchElementException>(() =>
            form.FindElement(By.Id("student-last-name-validation-message")));

        form.FindElement(By.Id("middle-name")).SendKeys(Keys.Space);
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.ThrowsException<NoSuchElementException>(() =>
            form.FindElement(By.Id("student-middle-name-validation-message")));

        form.FindElement(By.Id("middle-name")).SendKeys("Aa");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.AreEqual("Middle name must be at least 3 characters long.",
            form.FindElement(By.Id("student-middle-name-validation-message")).Text);

        form.FindElement(By.Id("middle-name")).SendKeys("Test");
        _webDriver.FindElement(By.TagName("h1")).Click();
        Assert.ThrowsException<NoSuchElementException>(() =>
            form.FindElement(By.Id("student-middle-name-validation-message")));
    }

    [TestMethod]
    public void AddStudentsEditFormIsValidatingPasswordCorrectly()
    {
        Login();
        ExpandNavbar();
        NavLinks.Find(x => x.Text.Equals("Add Student"))?.Click();

        var form = _webDriver.FindElement(By.TagName("form"));

        form.FindElement(By.Id("password")).SendKeys("a");
        Assert.AreEqual("Password must contain at least one uppercase letter.",
            form.FindElement(By.Id("student-password-validation-message")).Text);

        form.FindElement(By.Id("password")).SendKeys("A");
        Assert.AreEqual("Password must contain at least one number.",
            form.FindElement(By.Id("student-password-validation-message")).Text);
        
        form.FindElement(By.Id("password")).SendKeys("0");
        Assert.AreEqual("Password must contain at least one special character.",
            form.FindElement(By.Id("student-password-validation-message")).Text);
        
        form.FindElement(By.Id("password")).SendKeys(Keys.Add);
        Assert.AreEqual("Password must be at least 8 characters long.",
            form.FindElement(By.Id("student-password-validation-message")).Text);

        for (int i = 0; i < 7; i++)
        {
            form.FindElement(By.Id("password")).SendKeys("a");
        }
        
        Assert.ThrowsException<NoSuchElementException>(() =>
            form.FindElement(By.Id("student-password-validation-message")).Text);
    }
    
    
}
