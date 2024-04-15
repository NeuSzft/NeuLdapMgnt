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

    private const string SutHub = "http://selenium-hub:4444";
    private const string SutMngtDocker = "http://nginx-selenium:80";
    private const string SutMngtLocal = "http://localhost:8080";
    private static string _sutMngt = string.Empty;
    private static IWebDriver _webDriver = default!;
    private static WebDriverWait _wait = default!;

    private List<IWebElement> NavLinks
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

    private void Login()
    {
        _wait.Until(ExpectedConditions.UrlContains("/login"));
        _webDriver.FindElement(By.Id("username")).SendKeys(Username);
        _webDriver.FindElement(By.Id("password")).SendKeys(Password);
        _webDriver.FindElement(By.ClassName("btn-outline-primary")).Submit();
        _wait.Until(ExpectedConditions.TitleIs("Home"));
    }

    private void ExpandNavbar()
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

        _webDriver.Navigate().GoToUrl(_sutMngt + "/admins");
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);

        _webDriver.Navigate().GoToUrl(_sutMngt + "/admins/add");
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

        NavLinks.Find(x => x.Text.Equals("View Admins"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Admins"));
        Assert.AreEqual("Admins", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Add Admin"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Add Admin"));
        Assert.AreEqual("Add Admin", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Home"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Home"));
        Assert.AreEqual("Home", _webDriver.Title);

        NavLinks.Find(x => x.Text.Equals("Logout"))?.Click();
        _wait.Until(ExpectedConditions.TitleIs("Login"));
        Assert.AreEqual("Login", _webDriver.Title);
    }
}