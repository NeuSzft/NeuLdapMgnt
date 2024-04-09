using System.Net;

namespace NeuLdapMgnt.Api.Tests;

[TestClass]
public class AuthTests {
    [TestMethod]
    public async Task TestNoAuthorizationHeader() {
        var response = await RequestHelper.Client.GetAsync("/auth");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("Missing Authorization header.", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TestWrongAuthorizationHeader() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/auth").SetAuthHeader("Basic", "invalid");
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("Invalid Authorization header.", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TestWrongCredentials() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/auth").AuthWithBasic("testuser", "wrongpass");
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual("Wrong credentials.", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TestCorrectCredentialsAndResultToken() {
        var    request1  = new HttpRequestMessage(HttpMethod.Get, "/auth").AuthWithBasic("testuser", "password");
        var    response1 = await RequestHelper.Client.SendAsync(request1);
        string token     = await response1.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);

        var request2  = new HttpRequestMessage(HttpMethod.Get, "/testing/check-token").SetAuthHeader("Bearer", token);
        var response2 = await RequestHelper.Client.SendAsync(request2);

        Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);
    }

    [TestMethod]
    public async Task TestMissingToken() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/testing/check-token");
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual("Missing json web token.", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TestInvalidToken() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/testing/check-token").SetAuthHeader("Bearer", "invalidtoken");
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual("Invalid json web token.", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task TestExpiredToken() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/testing/check-token").AuthWithExpiredJwt();
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsTrue((await response.Content.ReadAsStringAsync()).Contains("expired"));
    }

    [TestMethod]
    public async Task TestValidToken() {
        var request  = new HttpRequestMessage(HttpMethod.Get, "/testing/check-token").AuthWithJwt();
        var response = await RequestHelper.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}