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
		private const string SutHub = "http://selenium-hub:4444/";
		private const string SutMngt = "http://nginx-selenium:80/";
		private IWebDriver _webDriver = null!;
		private WebDriverWait _wait = null!;

		[TestInitialize]
		public void InitializeTest()
		{
			var firefoxOptions = new FirefoxOptions();
			_webDriver = new RemoteWebDriver(new Uri(SutHub), firefoxOptions.ToCapabilities());
			_webDriver.Navigate().GoToUrl(SutMngt);
			_wait = new(_webDriver, TimeSpan.FromMilliseconds(3000));
		}

		[TestCleanup]
		public void CleanupTest()
		{
			_webDriver.Quit();
		}
		
		[TestMethod]
		public void RedirectionIsWorkingWhenUnauthorized()
		{
			_wait.Until(ExpectedConditions.UrlContains("login"));
			Assert.IsTrue(_webDriver.Url.Contains("login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "students");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "students/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "teachers");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "teachers/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "admins");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
			
			_webDriver.Navigate().GoToUrl(SutMngt + "admins/add");
			_wait.Until(ExpectedConditions.UrlContains("/login"));
			Assert.IsTrue(_webDriver.Url.Contains("/login"));
		}
	}
}
