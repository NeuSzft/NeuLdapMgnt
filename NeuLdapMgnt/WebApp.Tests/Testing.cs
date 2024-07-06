using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace NeuLdapMgnt.WebApp.Tests;

public static class Testing {
	public static readonly string Username     = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_NAME") ?? "admin";
	public static readonly string Password     = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD") ?? "adminpass";
	public static readonly bool   IsDockerized = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker";

	public static readonly Uri BaseUrl = Environment.GetEnvironmentVariable("WEBAPP_URL") is { Length: > 0 } env
		? new(env)
		: new("http://localhost:8080");

	private static readonly string         SeleniumHubUrl = Environment.GetEnvironmentVariable("SELENIUM_HUB_URL") ?? "http://selenium-hub:4444";
	private static readonly FirefoxOptions FirefoxOptions = new();

	public static readonly IWebDriver WebDriver = IsDockerized
		? new RemoteWebDriver(new Uri(SeleniumHubUrl), FirefoxOptions.ToCapabilities())
		: new FirefoxDriver(FirefoxOptions);

	public static readonly WebDriverWait Wait = new(WebDriver, TimeSpan.FromSeconds(5));

	public static Dictionary<string, IWebElement> NavLinks => WebDriver.FindElements(By.ClassName("nav-link")).ToDictionary(x => x.Text);

	public static void GoToPage() => WebDriver.Navigate().GoToUrl(BaseUrl);

	public static void GoToPage(string relativeUri) => WebDriver.Navigate().GoToUrl(new Uri(BaseUrl, relativeUri));

	public static void Login() {
		Wait.Until(ExpectedConditions.UrlContains("/login"));
		WebDriver.FindElement(By.Id("username")).SendKeys(Username);
		WebDriver.FindElement(By.Id("password")).SendKeys(Password);
		WebDriver.FindElement(By.ClassName("btn-outline-primary")).Submit();
		Wait.Until(ExpectedConditions.TitleIs("Home"));
		Wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("btn-close")));
		WebDriver.FindElement(By.ClassName("btn-close")).Click();
	}

	public static void ExpandNavbarItems() {
		foreach (IWebElement item in WebDriver.FindElements(By.ClassName("nav-item")).Where(x => x.Text is not ("Logout" or "Home"))) {
			Wait.Until(ExpectedConditions.ElementToBeClickable(item));
			item.Click();
		}
	}
}
