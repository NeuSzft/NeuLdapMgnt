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

	private const  string        SutMngtLocal = "http://localhost:8080";
	private static string        _sutMngt     = string.Empty;
	private static IWebDriver    _webDriver   = default!;
	private static WebDriverWait _wait        = default!;

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

		_wait = new(_webDriver, TimeSpan.FromMilliseconds(20000));
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
		_webDriver.FindElement(By.ClassName("btn-close")).Click();
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

		NavLinks.Find(x => x.Text.Equals("Administrators"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Administrators"));
		Assert.AreEqual("Administrators", _webDriver.Title);

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
			"Add Student"));

		Assert.IsTrue(_webDriver.FindElement(By.ClassName("btn-primary")).Text.Contains("Add Student"));
	}

	[TestMethod]
	public void AfterPressingAddStudentsButtonRedirectsToAddStudent()
	{
		Login();
		ExpandNavbar();

		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

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
		_wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));
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
			"Add Student"));

		_webDriver.FindElement(By.ClassName("btn-primary")).Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Student"));
		
		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.ArrowDown);
		Assert.AreEqual($"OM must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
			form.FindElement(By.Id("student-om-validation-message")).Text);

		for (int i = 0; i < 11; i++)
			form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.Backspace);

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
		Assert.AreEqual("First name is required.",
			form.FindElement(By.Id("student-first-name-validation-message")).Text);

		form.FindElement(By.Id("first-name")).SendKeys("Aa");
		Assert.AreEqual("First name must be at least 3 characters long.",
			form.FindElement(By.Id("student-first-name-validation-message")).Text);

		form.FindElement(By.Id("first-name")).SendKeys("Test");
		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-first-name-validation-message")));

		form.FindElement(By.Id("last-name")).SendKeys(Keys.Space);
		Assert.AreEqual("Last name is required.",
			form.FindElement(By.Id("student-last-name-validation-message")).Text);

		form.FindElement(By.Id("last-name")).SendKeys("Aa");
		Assert.AreEqual("Last name must be at least 3 characters long.",
			form.FindElement(By.Id("student-last-name-validation-message")).Text);

		form.FindElement(By.Id("last-name")).SendKeys("Test");
		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-last-name-validation-message")));

		form.FindElement(By.Id("middle-name")).SendKeys(Keys.Space);
		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-middle-name-validation-message")));

		form.FindElement(By.Id("middle-name")).SendKeys("Aa");
		Assert.AreEqual("Middle name must be at least 3 characters long.",
			form.FindElement(By.Id("student-middle-name-validation-message")).Text);

		form.FindElement(By.Id("middle-name")).SendKeys("Test");
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
			form.FindElement(By.Id("password")).SendKeys("a");

		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-password-validation-message")).Text);
	}

	[TestMethod]
	public void ClassCanBeAdded()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Classes"))?.Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.Id("add-class"))).Click();
		_webDriver.FindElement(By.Id("class-input")).SendKeys("9.a");
		_webDriver.FindElement(By.ClassName("btn-success")).Click();
		_webDriver.FindElement(By.Id("class-input")).SendKeys("10.a");
		_webDriver.FindElement(By.ClassName("btn-success")).Click();
		_webDriver.FindElement(By.ClassName("bi-x-lg")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		Assert.AreEqual("9.a", _webDriver.FindElement(By.CssSelector("tr > td")).Text);
	}

	[TestMethod]
	public void ClassDuplicateCannotBeAdded()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Classes"))?.Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.Id("add-class"))).Click();
		_webDriver.FindElement(By.Id("class-input")).SendKeys("9.a");
		_webDriver.FindElement(By.ClassName("btn-success")).Click();
		_webDriver.FindElement(By.ClassName("bi-x-lg")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		Assert.AreEqual(1,
			_webDriver.FindElements(By.CssSelector("tr > td")).Count(x => x.Text.Equals("9.a")));
	}

	[TestMethod]
	public void ClassCanBeDeleted()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Classes"))?.Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.Id("delete-class"))).Click();
		_webDriver.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals("9.a")).Click();
		_webDriver.FindElement(By.CssSelector("div.modal-footer > .btn-danger")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-danger")).Click();
		_webDriver.FindElement(By.ClassName("bi-x-lg")).Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));

		Assert.AreEqual(0,
			_webDriver.FindElements(By.CssSelector("tr > td")).Count(x => x.Text.Equals("9.a")));
	}

	[TestMethod]
	public void AddStudentCreatesStudent()
	{
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

		_webDriver.FindElement(By.ClassName("btn-primary")).Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Student"));
		
		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, _webDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count);
	}

	[TestMethod]
	public void CannotAddDuplicateStudent()
	{
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Student"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Student"));

		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, _webDriver.FindElements(By.CssSelector(".toast-container .bg-danger")).Count);
	}

	[TestMethod]
	public void CreatedStudentAppearsInTable()
	{
		string id        = "70000000000";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
		Assert.AreEqual($"{firstName} {lastName}", tds[2].Text);
		Assert.AreEqual(cls, tds[3].Text);
	}

	[TestMethod]
	public void StudentStatusCanBeSetToInactive()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Active] students",
			_webDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void InactiveStudentAppearsInTable()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		_webDriver.FindElement(By.ClassName("btn-outline-dark")).Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(1, tds.Count(x => x.Text.Equals("70000000000")));
	}

	[TestMethod]
	public void StudentCanBePermanentlyDeleted()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		_webDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Inactive] users",
			_webDriver.FindElement(By.TagName("h3")).Text);

		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Active] students",
			_webDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void AddTeacherCreatesTeacher()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.TextToBePresentInElement(_webDriver.FindElement(By.ClassName("btn-primary")),
			"Add Teacher"));

		_webDriver.FindElement(By.ClassName("btn-primary")).Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Teacher"));
		
		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("teacher-id")).SendKeys(id);
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, _webDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count);
	}

	[TestMethod]
	public void CannotAddDuplicateTeacher()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Teacher"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Teacher"));

		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("teacher-id")).SendKeys(id);
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElement(By.CssSelector($"option[value='{cls}']")).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, _webDriver.FindElements(By.CssSelector(".toast-container .bg-danger")).Count);
	}

	[TestMethod]
	public void CreatedTeacherAppearsInTable()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
		Assert.AreEqual($"{firstName} {lastName}", tds[2].Text);
		Assert.AreEqual(cls, tds[3].Text);
	}

	[TestMethod]
	public void TeacherStatusCanBeSetToInactive()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Active] teachers",
			_webDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void InactiveTeacherAppearsInTable()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		_webDriver.FindElement(By.ClassName("btn-outline-dark")).Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(1, tds.Count(x => x.Text.Equals("john.doe")));
	}

	[TestMethod]
	public void TeacherCanBePermanentlyDeleted()
	{
		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		_webDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Inactive] users",
			_webDriver.FindElement(By.TagName("h3")).Text);

		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Active] teachers",
			_webDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void TeacherStatusCanBeSetToAdmin()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Teacher"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Teacher"));

		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("teacher-id")).SendKeys(id);
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));

		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Assert.IsTrue(_webDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count > 0);

		NavLinks.Find(x => x.Text.Equals("Administrators"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
	}

	[TestMethod]
	public void AdminCanBeDeletedButTeacherStillExists()
	{
		Login();
		ExpandNavbar();

		NavLinks.Find(x => x.Text.Equals("Administrators"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		_webDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));

		var tds = _webDriver.FindElements(By.CssSelector("table > tbody > tr > td"));
		Assert.AreEqual("john.doe", tds[1].Text);
	}

	[TestMethod]
	public void DeletingTeacherPermanentlyDeletesFromAdmins()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Teacher"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Teacher"));

		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("teacher-id")).SendKeys(id);
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));

		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElements(By.CssSelector(".form-switch > input")).FirstOrDefault()?.Click();
		_webDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		foreach (var element in _webDriver.FindElements(By.ClassName("btn-close")))
			element.Click();

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		_webDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no [Active] teachers",
			_webDriver.FindElement(By.TagName("h3")).Text);

		NavLinks.Find(x => x.Text.Equals("Administrators"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no teachers with [Administrator] status",
			_webDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void StudentCanBeInspectedFromStudentsPage()
	{
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Student"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		
		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		_webDriver.FindElement(By.ClassName("bi-eye")).Click();
		_wait.Until(ExpectedConditions.TitleIs($"{firstName} {lastName}"));

		form = _webDriver.FindElement(By.TagName("form"));
		Assert.AreEqual("70000000000", form.FindElement(By.CssSelector("input[type='number']")).GetAttribute("value"));
		Assert.AreEqual(firstName, form.FindElement(By.Id("first-name")).GetAttribute("value"));
		Assert.AreEqual(lastName, form.FindElement(By.Id("last-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void StudentCanBeInspectedFromInactiveUsersPage()
	{
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Students"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		_webDriver.FindElement(By.ClassName("bi-eye")).Click();
		_wait.Until(ExpectedConditions.TitleIs($"{firstName} {lastName}"));

		var form = _webDriver.FindElement(By.TagName("form"));
		Assert.AreEqual("70000000000", form.FindElement(By.CssSelector("input[type='number']")).GetAttribute("value"));
		Assert.AreEqual(firstName, form.FindElement(By.Id("first-name")).GetAttribute("value"));
		Assert.AreEqual(lastName, form.FindElement(By.Id("last-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void TeacherCanBeInspectedFromTeachersPage()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("Add Teacher"))?.Click();
		_wait.Until(ExpectedConditions.TitleIs("Add Teacher"));

		var form = _webDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("teacher-id")).SendKeys(id);
		form.FindElement(By.Id("first-name")).SendKeys(firstName);
		form.FindElement(By.Id("last-name")).SendKeys(lastName);
		form.FindElement(By.Id("class-select")).Click();
		_webDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();

		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		_webDriver.FindElements(By.ClassName("bi-eye")).LastOrDefault()?.Click();
		_wait.Until(ExpectedConditions.TitleIs($"{firstName} {lastName}"));

		form = _webDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("teacher-id")).GetAttribute("value"));
		Assert.AreEqual(firstName, form.FindElement(By.Id("first-name")).GetAttribute("value"));
		Assert.AreEqual(lastName, form.FindElement(By.Id("last-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void TeacherCanBeInspectedFromAdminsPage()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		NavLinks.Find(x => x.Text.Equals("Administrators"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		_webDriver.FindElements(By.ClassName("bi-eye")).LastOrDefault()?.Click();
		_wait.Until(ExpectedConditions.TitleIs($"{firstName} {lastName}"));

		var form = _webDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("teacher-id")).GetAttribute("value"));
		Assert.AreEqual(firstName, form.FindElement(By.Id("first-name")).GetAttribute("value"));
		Assert.AreEqual(lastName, form.FindElement(By.Id("last-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void TeacherCanBeInspectedFromInactiveUsersPage()
	{
		string id        = "john.doe";
		string firstName = "John";
		string lastName  = "Doe";
		string cls       = "10.a";

		Login();
		ExpandNavbar();
		NavLinks.Find(x => x.Text.Equals("View Teachers"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		_webDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		_wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		_webDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		_webDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		_webDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		NavLinks.Find(x => x.Text.Equals("Inactive Users"))?.Click();
		_wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		_webDriver.FindElements(By.ClassName("bi-eye")).LastOrDefault()?.Click();
		_wait.Until(ExpectedConditions.TitleIs($"{firstName} {lastName}"));

		var form = _webDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("teacher-id")).GetAttribute("value"));
		Assert.AreEqual(firstName, form.FindElement(By.Id("first-name")).GetAttribute("value"));
		Assert.AreEqual(lastName, form.FindElement(By.Id("last-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}
}
