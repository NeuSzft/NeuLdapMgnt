using NeuLdapMgnt.Models;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace NeuLdapMgnt.WebApp.Tests
{
	[TestClass]
	public class SeleniumTests
	{
		private static readonly string EnvironmentMode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";
		private static readonly string SutHub = Environment.GetEnvironmentVariable("DEFAULT_SELENIUM_HUB_URL") ?? "http://selenium-hub:4444";
		private static readonly string SutMngtDocker = Environment.GetEnvironmentVariable("DEFAULT_MANAGEMENT_URL") ??  "http://nginx-selenium:80";
		private const string SutMngtLocal = "http://localhost:8080";
		private string SutMngt = string.Empty;
		private static readonly string Username = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME") ?? "admin";
		private static readonly string Password = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? "adminpass";

		private IWebDriver _webDriver = null!;
		private WebDriverWait _wait = null!;

		[TestInitialize]
		public void InitializeTest()
		{
			var firefoxOptions = new FirefoxOptions();
			if (EnvironmentMode.Equals("Docker"))
			{
				SutMngt = SutMngtDocker;
				_webDriver = new RemoteWebDriver(new Uri(SutHub), firefoxOptions.ToCapabilities());
				_webDriver.Navigate().GoToUrl(SutMngt);
			}
			else
			{
				SutMngt = SutMngtLocal;
				_webDriver = new FirefoxDriver(firefoxOptions);
				_webDriver.Navigate().GoToUrl(SutMngtLocal);
			}
			_wait = new(_webDriver, TimeSpan.FromMilliseconds(5000));
		}

		[TestCleanup]
		public void CleanupTest()
		{
			_webDriver.Quit();
		}

		[TestMethod]
		public void DefaultRedirectionToLoginPage()
		{
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
		}
		
		[TestMethod]
		public void RedirectionIsWorkingWhenUnauthorized()
		{
			_webDriver.Navigate().GoToUrl(SutMngt + "/students");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "/students/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "/teachers");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "/teachers/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "/admins");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "/admins/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
		}
		
		[TestMethod]
		public void SuccessfulLoginRedirectsToHomePage()
		{
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			
			_webDriver.FindElement(By.Id("username")).SendKeys(Username);
			_webDriver.FindElement(By.Id("password")).SendKeys(Password);
			_webDriver.FindElement(By.ClassName("btn-outline-primary")).Submit();
			_wait.Until(ExpectedConditions.TitleIs("Home"));
			Assert.AreEqual("Home", _webDriver.Title);
		}
	}
}
