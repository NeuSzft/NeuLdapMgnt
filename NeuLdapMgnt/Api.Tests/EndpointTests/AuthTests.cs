using System.Net;

namespace NeuLdapMgnt.Api.Tests.EndpointTests;

[TestClass]
public class AuthTests {
	[TestMethod]
	public async Task TestNoAuthorizationHeader() {
		var response = await Testing.Client.GetAsync("/api/auth");

		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.AreEqual("Missing Authorization header.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongAuthorizationHeader() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").SetAuthHeader("Basic", "invalid");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.AreEqual("Invalid Authorization header.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongAdminUsername() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("wrongadmin", "adminpass");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("Wrong credentials.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestWrongAdminPassword() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("admin", "wrongpass");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("Wrong credentials.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestCorrectAdminCredentialsAndResultToken() {
		var    request1  = new HttpRequestMessage(HttpMethod.Get, "/api/auth").AuthWithBasic("admin", "adminpass");
		var    response1 = await Testing.Client.SendAsync(request1);
		string token     = await response1.Content.ReadAsStringAsync();

		Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);

		var request2  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").SetAuthHeader("Bearer", token);
		var response2 = await Testing.Client.SendAsync(request2);

		Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
	}

	[TestMethod]
	public async Task TestMissingToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("Missing json web token.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestInvalidToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").SetAuthHeader("Bearer", "invalidtoken");
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.AreEqual("Invalid json web token.", await response.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task TestExpiredToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").AuthWithExpiredJwt();
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
		Assert.IsTrue((await response.Content.ReadAsStringAsync()).Contains("expired"));
	}

	[TestMethod]
	public async Task TestValidToken() {
		var request  = new HttpRequestMessage(HttpMethod.Get, "/api/testing/check-token").AuthWithJwt();
		var response = await Testing.Client.SendAsync(request);

		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}
}
