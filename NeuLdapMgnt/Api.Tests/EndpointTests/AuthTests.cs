using System.Net;

namespace NeuLdapMgnt.Api.Tests.EndpointTests;

[TestClass]
public class AuthTests {
	[ClassInitialize]
	public static void Init(TestContext context) {
		Employee employee = new() {
			Id            = "george.sears",
			Uid           = 4000,
			Gid           = 4000,
			Class         = "-",
			Username      = "geosea",
			GivenName     = "George",
			Surname       = "Sears",
			MiddleName    = null,
			Email         = "solidus@mail.com",
			HomeDirectory = "/home/geosea",
			Password      = Utils.BCryptHashPassword("lalilulelo"),
			FullName      = "George Sears",
			IsTeacher     = true,
			IsAdmin       = true
		};

		Testing.LdapService.TryAddEntity(employee, employee.Id, true).AssertSuccess();
	}

	[ClassCleanup]
	public static void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public async Task TestNoAuthorizationHeader() {
		var response = await Testing.Client.GetAsync("/api/auth");

		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.AreEqual("400: Missing Authorization header", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongAuthorizationHeader() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").SetAuthHeader("Basic", "invalid");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.AreEqual("400: Invalid Authorization header", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongUsername() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("wrongadmin", "adminpass");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("401: Wrong credentials", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongDefaultAdminPassword() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("admin", "wrongpass");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("401: Wrong credentials", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestCorrectDefaultAdminCredentialsAndResultToken() {
		var    request1  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("admin", "adminpass");
		var    response1 = await Testing.Client.SendAsync(request1);
		string token     = await response1.Content.ReadAsStringAsync();

		Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);

		var request2  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").SetAuthHeader("Bearer", token);
		var response2 = await Testing.Client.SendAsync(request2);

		Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
	}

	[TestMethod]
	public async Task TestWrongAdminPassword() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("george.sears", "wrongpass");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("401: Wrong credentials", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestCorrectAdminCredentialsAndResultToken() {
		var    request1  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("george.sears", "lalilulelo");
		var    response1 = await Testing.Client.SendAsync(request1);
		string token     = await response1.Content.ReadAsStringAsync();

		Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);

		var request2  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").SetAuthHeader("Bearer", token);
		var response2 = await Testing.Client.SendAsync(request2);

		Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
	}

	[TestMethod]
	public async Task TestMissingAuthHeader() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.AreEqual("400: Missing authorization header", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestMalformedToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").SetAuthHeader("Bearer", "invalidtoken");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("401: Malformed json web token", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestExpiredToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").AuthWithExpiredJwt();
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("401: Expired json web token", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestValidToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").AuthWithJwt();
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}
}
