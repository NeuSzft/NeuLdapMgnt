namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

file class Dummy : Person {
	public override int Uid { get; set; }
	public override int Gid { get; set; }
}

[TestClass]
public class UtilsTests {
	[TestMethod]
	public void TestGetEnv() {
		const string env         = "TEST_ENV";
		const string envValue    = "something";
		const string expectedMsg = $"The '{env}' environment variable is not defined";

		Environment.SetEnvironmentVariable(env, envValue);
		Assert.AreEqual(Utils.GetEnv(env), envValue);

		Environment.SetEnvironmentVariable(env, string.Empty);
		Assert.ThrowsException<ApplicationException>(() => Utils.GetEnv(env), expectedMsg);

		Environment.SetEnvironmentVariable(env, "\t\n\r ");
		Assert.ThrowsException<ApplicationException>(() => Utils.GetEnv(env), expectedMsg);
	}

	[TestMethod]
	[DataRow("something", false)]
	[DataRow("", true)]
	[DataRow("\t\n\r ", true)]
	public void TestGetEnvWithFallback(string value, bool willFallback) {
		const string env      = "TEST_ENV";
		const string fallback = "fallback";

		Environment.SetEnvironmentVariable(env, value);
		Assert.AreEqual(willFallback ? fallback : value, Utils.GetEnv(env, fallback));
	}

	[TestMethod]
	[DataRow("true", true)]
	[DataRow("True", true)]
	[DataRow("truE", true)]
	[DataRow("TRUE", true)]
	[DataRow("   true   ", true)]
	[DataRow("\ttrue\r\n", true)]
	[DataRow("true_", false)]
	[DataRow("false", false)]
	[DataRow("False", false)]
	[DataRow("asdf", false)]
	[DataRow("1", false)]
	public void TestIsEnvTrue(string value, bool expected) {
		const string env = "TEST_ENV";

		Environment.SetEnvironmentVariable(env, value);
		Assert.AreEqual(Utils.IsEnvTrue(env), expected);
	}

	[TestMethod]
	public void TestSetAndCheckPassword() {
		const string password = "Password_1";

		Dummy dummy = new();
		dummy.SetPassword(password);

		Assert.IsTrue(dummy.CheckPassword(password));
		Assert.IsFalse(dummy.CheckPassword("asdf"));
	}
}
