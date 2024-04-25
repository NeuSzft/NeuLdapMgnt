using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class ValueExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestTrySetValue() {
		Assert.IsFalse(Testing.LdapService.ValueExists("dummy-value"));

		Assert.IsTrue(Testing.LdapService.SetValue("dummy-value", "123", out var error));
		Assert.IsNull(error);

		Assert.IsTrue(Testing.LdapService.ValueExists("dummy-value"));

		Assert.IsFalse(Testing.LdapService.ValueExists("missing-value"));
	}

	[TestMethod]
	public void TestTrySetValueEmptyValue() {
		Assert.IsFalse(Testing.LdapService.ValueExists("dummy-value"));

		Assert.IsFalse(Testing.LdapService.SetValue("dummy-value", string.Empty, out var error));
		Assert.IsNotNull(error);

		Assert.IsFalse(Testing.LdapService.ValueExists("dummy-value"));
	}

	[TestMethod]
	public void TestTrySetAndGetValue() {
		Assert.IsTrue(Testing.LdapService.SetValue("dummy-value", "123", out var error));
		Assert.IsNull(error);

		Assert.AreEqual("123", Testing.LdapService.GetValue("dummy-value", out error));
		Assert.IsNull(error);

		Assert.IsTrue(Testing.LdapService.SetValue("dummy-value", "456", out error));
		Assert.IsNull(error);

		Assert.AreEqual("456", Testing.LdapService.GetValue("dummy-value", out error));
		Assert.IsNull(error);

		Assert.IsNull(Testing.LdapService.GetValue("missing-value", out error));
		Assert.IsNotNull(error);
	}

	[TestMethod]
	public void TestTryUnsetValue() {
		Assert.IsTrue(Testing.LdapService.SetValue("dummy-value", "123", out var error));
		Assert.IsNull(error);

		Assert.AreEqual("123", Testing.LdapService.GetValue("dummy-value", out error));
		Assert.IsNull(error);

		Assert.IsTrue(Testing.LdapService.UnsetValue("dummy-value", out error));
		Assert.IsNull(error);

		Assert.IsNull(Testing.LdapService.GetValue("dummy-value", out error));
		Assert.IsNotNull(error);

		Assert.IsFalse(Testing.LdapService.UnsetValue("missing-value", out error));
		Assert.IsNull(error);

		Assert.IsTrue(Testing.LdapService.UnsetValue("missing-value", out error, true));
		Assert.IsNull(error);
	}

	[TestMethod]
	public void TestTryGetAllValues() {
		Dictionary<string, string> values = new() {
			{ "dummy-value1", "123" },
			{ "dummy-value2", "456" },
			{ "dummy-value3", "789" }
		};

		string? error;
		foreach (var value in values) {
			Assert.IsTrue(Testing.LdapService.SetValue(value.Key, value.Value, out error));
			Assert.IsNull(error);
		}

		CollectionAssert.AreEqual(values, Testing.LdapService.GetAllValues(out error));
		Assert.IsNull(error);
	}
}
