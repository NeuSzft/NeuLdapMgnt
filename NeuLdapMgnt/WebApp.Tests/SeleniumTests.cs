using NeuLdapMgnt.Models;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;

namespace NeuLdapMgnt.WebApp.Tests;

[TestClass]
public class SeleniumTests {
	[TestInitialize]
	public void InitializeTest() => Testing.GoToPage();

	[TestMethod]
	public void DefaultRedirectionToLoginPage()
	{
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);
	}

	[TestMethod]
	public void RedirectionIsWorkingWhenUnauthorized()
	{
		Testing.GoToPage("/students");
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);

		Testing.GoToPage("/students/add");
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);

		Testing.GoToPage("/employees");
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);

		Testing.GoToPage("/employees/add");
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);

		Testing.GoToPage("/db/admins");
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);
	}

	[TestMethod]
	public void SuccessfulLoginRedirectsToHomePage()
	{
		Testing.Login();
		Assert.AreEqual("Home", Testing.WebDriver.Title);
	}

	[TestMethod]
	public void NavbarLinksAreWorking()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Students"));
		Assert.AreEqual("Students", Testing.WebDriver.Title);

		Testing.NavLinks["Add Student"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));
		Assert.AreEqual("Add Student", Testing.WebDriver.Title);

		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Employees"));
		Assert.AreEqual("Employees", Testing.WebDriver.Title);

		Testing.NavLinks["Add Employee"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));
		Assert.AreEqual("Add Employee", Testing.WebDriver.Title);

		Testing.NavLinks["Administrators"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Administrators"));
		Assert.AreEqual("Administrators", Testing.WebDriver.Title);

		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Inactive Users"));
		Assert.AreEqual("Inactive Users", Testing.WebDriver.Title);

		Testing.NavLinks["Classes"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Classes"));
		Assert.AreEqual("Classes", Testing.WebDriver.Title);

		Testing.NavLinks["Request Logs"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Request Logs"));
		Assert.AreEqual("Request Logs", Testing.WebDriver.Title);

		Testing.NavLinks["Import/Export"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Database Import/Export"));
		Assert.AreEqual("Database Import/Export", Testing.WebDriver.Title);

		Testing.NavLinks["Default Admin"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Default Admin Settings"));
		Assert.AreEqual("Default Admin Settings", Testing.WebDriver.Title);

		Testing.NavLinks["Home"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Home"));
		Assert.AreEqual("Home", Testing.WebDriver.Title);

		Testing.NavLinks["Logout"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Log In"));
		Assert.AreEqual("Log In", Testing.WebDriver.Title);
	}

	[TestMethod]
	public void NoStudentsArePresent()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Students"));
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.TagName("h3")),
			"There are no Active students"));

		Assert.AreEqual("There are no Active students", Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void NoStudentsArePresentAndAddStudentsButtonIsPresent()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

		Assert.IsTrue(Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Text.Contains("Add Student"));
	}

	[TestMethod]
	public void AfterPressingAddStudentsButtonRedirectsToAddStudent()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

		Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));
		Assert.AreEqual("Add Student", Testing.WebDriver.Title);
	}

	[TestMethod]
	public void AddStudentsEditFormHasLoadedDefaultValues()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));
		Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(Student.IdMinValue.ToString(),
			form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).GetAttribute("value"));

		Assert.AreEqual(string.Empty, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(string.Empty, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.AreEqual(string.Empty, form.FindElement(By.Id("middle-name")).GetAttribute("value"));
		Assert.AreEqual(string.Empty, form.FindElement(By.Id("class-select")).GetAttribute("value"));
		Assert.AreEqual(string.Empty, form.FindElement(By.Id("student-password")).GetAttribute("value"));
		Assert.AreEqual(string.Empty, form.FindElement(By.Id("student-confirm-password")).GetAttribute("value"));
	}

	[TestMethod]
	public void AddStudentsEditFormIsValidatingOmCorrectly()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

		Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.ArrowDown);
		Assert.AreEqual($"ID must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
			form.FindElement(By.Id("student-id-validation-message")).Text);

		for (int i = 0; i < 11; i++)
			form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys(Keys.Backspace);

		form.FindElement(By.CssSelector("div:nth-child(2) > .form-control")).SendKeys("80000000000");
		Assert.AreEqual($"ID must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
			form.FindElement(By.Id("student-id-validation-message")).Text);
	}

	[TestMethod]
	public void AddStudentsEditFormIsValidatingFullNameCorrectly()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Student"].Click();

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("surname")).SendKeys(Keys.Space);
		Assert.AreEqual("Surname is required.",
			form.FindElement(By.Id("student-surname-validation-message")).Text);

		form.FindElement(By.Id("surname")).SendKeys("Aa");
		Assert.AreEqual("Surname must be at least 3 characters long.",
			form.FindElement(By.Id("student-surname-validation-message")).Text);

		form.FindElement(By.Id("surname")).SendKeys("Test");
		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-surname-validation-message")));

		form.FindElement(By.Id("given-name")).SendKeys(Keys.Space);
		Assert.AreEqual("Given name is required.",
			form.FindElement(By.Id("student-given-name-validation-message")).Text);

		form.FindElement(By.Id("given-name")).SendKeys("Aa");
		Assert.AreEqual("Given name must be at least 3 characters long.",
			form.FindElement(By.Id("student-given-name-validation-message")).Text);

		form.FindElement(By.Id("given-name")).SendKeys("Test");
		Assert.ThrowsException<NoSuchElementException>(() =>
			form.FindElement(By.Id("student-given-name-validation-message")));

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
	public void AddStudentsEditFormIsValidatingPasswordCorrectly() {
		By passwordInput  = By.Id("student-password");
		By passwordValMsg = By.Id("student-password-validation-message");
		By confirmInput   = By.Id("student-confirm-password");
		By confirmValMsg  = By.Id("student-confirm-password-validation-message");

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Student"].Click();

		var form = Testing.WebDriver.FindElement(By.TagName("form"));

		form.FindElement(passwordInput).SendKeys("a");
		Assert.AreEqual("Password must contain at least one uppercase letter.", form.FindElement(passwordValMsg).Text);

		form.FindElement(passwordInput).SendKeys("A");
		Assert.AreEqual("Password must contain at least one number.", form.FindElement(passwordValMsg).Text);

		form.FindElement(passwordInput).SendKeys("0");
		Assert.AreEqual("Password must contain at least one special character.", form.FindElement(passwordValMsg).Text);

		form.FindElement(passwordInput).SendKeys("+");
		Assert.AreEqual("Password must be at least 8 characters long.", form.FindElement(passwordValMsg).Text);

		for (int i = 0; i < 7; i++)
			form.FindElement(passwordInput).SendKeys("a");
		Assert.ThrowsException<NoSuchElementException>(() => form.FindElement(passwordValMsg).Text);

		Assert.AreEqual("Passwords do not match.", form.FindElement(confirmValMsg).Text);

		form.FindElement(confirmInput).SendKeys("aA0+aaaaaaa");
		Assert.ThrowsException<NoSuchElementException>(() => form.FindElement(confirmValMsg).Text);
	}

	[TestMethod]
	public void ClassCanBeAdded()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Classes"].Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("add-class"))).Click();
		Testing.WebDriver.FindElement(By.Id("class-input")).SendKeys("9.a");
		Testing.WebDriver.FindElement(By.ClassName("btn-success")).Click();
		Testing.WebDriver.FindElement(By.Id("class-input")).SendKeys("10.a");
		Testing.WebDriver.FindElement(By.ClassName("btn-success")).Click();
		Testing.WebDriver.FindElement(By.ClassName("bi-x-lg")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		Assert.AreEqual("9.a", Testing.WebDriver.FindElement(By.CssSelector("tr > td")).Text);
	}

	[TestMethod]
	public void ClassDuplicateCannotBeAdded()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Classes"].Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("add-class"))).Click();
		Testing.WebDriver.FindElement(By.Id("class-input")).SendKeys("9.a");
		Testing.WebDriver.FindElement(By.ClassName("btn-success")).Click();
		Testing.WebDriver.FindElement(By.ClassName("bi-x-lg")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		Assert.AreEqual(1,
			Testing.WebDriver.FindElements(By.CssSelector("tr > td")).Count(x => x.Text.Equals("9.a")));
	}

	[TestMethod]
	public void ClassCanBeDeleted()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Classes"].Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("delete-class"))).Click();
		Testing.WebDriver.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals("9.a")).Click();
		Testing.WebDriver.FindElement(By.CssSelector("div.modal-footer > .btn-danger")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-danger")).Click();
		Testing.WebDriver.FindElement(By.ClassName("bi-x-lg")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));

		Assert.AreEqual(0,
			Testing.WebDriver.FindElements(By.CssSelector("tr > td")).Count(x => x.Text.Equals("9.a")));
	}

	[TestMethod]
	public void AddStudentCreatesStudent()
	{
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Student"));

		Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, Testing.WebDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count);
	}

	[TestMethod]
	public void CannotAddDuplicateStudent()
	{
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Student"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Student"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.IsTrue(Testing.WebDriver.FindElements(By.CssSelector(".toast-container .bg-danger")).Count != 0);
	}

	[TestMethod]
	public void CreatedStudentAppearsInTable()
	{
		string id        = "70000000000";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
		Assert.AreEqual($"{givenName} {surName}", tds[2].Text);
		Assert.AreEqual(cls, tds[3].Text);
	}

	[TestMethod]
	public void StudentStatusCanBeSetToInactive()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Active students",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void InactiveStudentAppearsInTable()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		Testing.WebDriver.FindElement(By.ClassName("btn-outline-dark")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(1, tds.Count(x => x.Text.Equals("70000000000")));
	}

	[TestMethod]
	public void StudentCanBePermanentlyDeleted()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Inactive users",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);

		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Active students",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void AddEmployeeCreatesEmployee()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.TextToBePresentInElement(Testing.WebDriver.FindElement(By.ClassName("btn-primary")),
			"Add Employee"));

		Testing.WebDriver.FindElement(By.ClassName("btn-primary")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("employee-id")).SendKeys(id);
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.CssSelector(".btn-outline-primary")).Click();
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.AreEqual(1, Testing.WebDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count);
	}

	[TestMethod]
	public void CannotAddDuplicateEmployee()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Employee"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("employee-id")).SendKeys(id);
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.CssSelector(".btn-outline-primary")).Click();
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElement(By.CssSelector($"option[value='{cls}']")).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Assert.IsTrue(Testing.WebDriver.FindElements(By.CssSelector(".toast-container .bg-danger")).Count != 0);
	}

	[TestMethod]
	public void CreatedEmployeeAppearsInTable()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
		Assert.AreEqual($"{givenName} {surName}", tds[2].Text);
		Assert.AreEqual(cls, tds[4].Text);
	}

	[TestMethod]
	public void EmployeeStatusCanBeSetToInactive()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Active employees",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void InactiveEmployeeAppearsInTable()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		Testing.WebDriver.FindElement(By.ClassName("btn-outline-dark")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(1, tds.Count(x => x.Text.Equals("john.doe")));
	}

	[TestMethod]
	public void EmployeeCanBePermanentlyDeleted()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Inactive users",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);

		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Active employees",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void EmployeeStatusCanBeSetToAdmin()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Employee"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("employee-id")).SendKeys(id);
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.CssSelector(".btn-outline-primary")).Click();
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));

		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Assert.IsTrue(Testing.WebDriver.FindElements(By.CssSelector(".toast-container .bg-success")).Count > 0);

		Testing.NavLinks["Administrators"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td")).ToList();
		Assert.AreEqual(id, tds[1].Text);
	}

	[TestMethod]
	public void AdminCanBeDeletedButEmployeeStillExists()
	{
		Testing.Login();
		Testing.ExpandNavbarItems();

		Testing.NavLinks["Administrators"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));

		var tds = Testing.WebDriver.FindElements(By.CssSelector("table > tbody > tr > td"));
		Assert.AreEqual("john.doe", tds[1].Text);
	}

	[TestMethod]
	public void DeletingEmployeePermanentlyDeletesFromAdmins()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Employee"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("employee-id")).SendKeys(id);
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.CssSelector(".btn-outline-primary")).Click();
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));

		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElements(By.CssSelector(".form-switch > input")).FirstOrDefault()?.Click();
		Testing.WebDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		foreach (var element in Testing.WebDriver.FindElements(By.ClassName("btn-close")))
			element.Click();

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-outline-danger")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-outline-danger")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));

		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no Active employees",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);

		Testing.NavLinks["Administrators"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("h3")));
		Assert.AreEqual("There are no employees with Administrator status",
			Testing.WebDriver.FindElement(By.TagName("h3")).Text);
	}

	[TestMethod]
	public void StudentCanBeInspectedFromStudentsPage()
	{
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Student"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Testing.NavLinks["View Students"].Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		Testing.WebDriver.FindElement(By.ClassName("bi-eye")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs($"{givenName} {surName}"));

		form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual("70000000000", form.FindElement(By.CssSelector("input[type='number']")).GetAttribute("value"));
		Assert.AreEqual(surName, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(givenName, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void StudentCanBeInspectedFromInactiveUsersPage()
	{
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Students"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		Testing.WebDriver.FindElement(By.ClassName("bi-eye")).Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs($"{givenName} {surName}"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual("70000000000", form.FindElement(By.CssSelector("input[type='number']")).GetAttribute("value"));
		Assert.AreEqual(surName, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(givenName, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void EmployeeCanBeInspectedFromEmployeesPage()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["Add Employee"].Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs("Add Employee"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		form.FindElement(By.Id("employee-id")).SendKeys(id);
		form.FindElement(By.Id("surname")).SendKeys(surName);
		form.FindElement(By.Id("given-name")).SendKeys(givenName);
		form.FindElement(By.CssSelector(".btn-outline-primary")).Click();
		form.FindElement(By.Id("class-select")).Click();
		Testing.WebDriver.FindElements(By.TagName("option")).First(x => x.Text.Equals(cls)).Click();
		form.Submit();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("form")));
		Testing.NavLinks["View Employees"].Click();

		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		Testing.WebDriver.FindElements(By.ClassName("bi-eye")).LastOrDefault()?.Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs($"{givenName} {surName}"));

		form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("employee-id")).GetAttribute("value"));
		Assert.AreEqual(surName, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(givenName, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void EmployeeCanBeInspectedFromAdminsPage()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElements(By.CssSelector(".form-switch > input")).LastOrDefault()?.Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.NavLinks["Administrators"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		Testing.WebDriver.FindElements(By.ClassName("bi-eye")).LastOrDefault()?.Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs($"{givenName} {surName}"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("employee-id")).GetAttribute("value"));
		Assert.AreEqual(surName, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(givenName, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}

	[TestMethod]
	public void EmployeeCanBeInspectedFromInactiveUsersPage()
	{
		string id        = "john.doe";
		string surName = "John";
		string givenName  = "Doe";
		string cls       = "10.a";

		Testing.Login();
		Testing.ExpandNavbarItems();
		Testing.NavLinks["View Employees"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));

		Testing.WebDriver.FindElement(By.CssSelector("table > tbody > tr > td > input")).Click();
		Testing.Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn-warning")));
		Testing.WebDriver.FindElement(By.CssSelector(".btn-warning")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".form-switch > input")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-dialog .btn-success")).Click();
		Testing.WebDriver.FindElement(By.CssSelector(".modal-confirmation .btn-success")).Click();

		Testing.NavLinks["Inactive Users"].Click();
		Testing.Wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("table")));
		Testing.WebDriver.FindElements(By.ClassName("bi-eye")).FirstOrDefault()?.Click();
		Testing.Wait.Until(ExpectedConditions.TitleIs($"{givenName} {surName}"));

		var form = Testing.WebDriver.FindElement(By.TagName("form"));
		Assert.AreEqual(id, form.FindElement(By.Id("employee-id")).GetAttribute("value"));
		Assert.AreEqual(surName, form.FindElement(By.Id("surname")).GetAttribute("value"));
		Assert.AreEqual(givenName, form.FindElement(By.Id("given-name")).GetAttribute("value"));
		Assert.IsTrue(form.FindElement(By.Id("class-select")).Text.Contains(cls));
	}
}
