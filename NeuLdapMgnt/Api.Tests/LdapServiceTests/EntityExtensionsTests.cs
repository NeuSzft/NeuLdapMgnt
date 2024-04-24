namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class EntityExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestTryAddEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		foreach (Dummy dummy in dummies)
			Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertSuccess();

		foreach (var dummy in dummies)
			Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>(dummy.Id.ToString()));
	}

	[TestMethod]
	public void TestTryAddDuplicateEntity() {
		Dummy dummy = Dummy.CreateDummies(1).First();

		Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertSuccess();
		Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertFailure();

		Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>(dummy.Id.ToString()));
	}

	[TestMethod]
	public void TestTryAddAndGetEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		foreach (Dummy dummy in dummies)
			Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertSuccess();

		foreach (var dummy in dummies) {
			var result = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString(), true).AssertSuccess();
			Assert.AreEqual(dummy, result.GetValue());
		}
	}

	[TestMethod]
	public void TestTryAddAndGetEntities() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true).AssertSuccess();

		var result = Testing.LdapService.GetAllEntities<Dummy>(true).AssertSuccess();

		Assert.AreEqual(dummies.Length, result.Values.Length);
		CollectionAssert.AreEqual(dummies, result.Values);
	}

	[TestMethod]
	public void TestTryDeleteEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true).AssertSuccess();

		Testing.LdapService.TryDeleteEntity<Dummy>("3").AssertSuccess();
		Testing.LdapService.TryDeleteEntity<Dummy>("5").AssertFailure();

		Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>("0"));
		Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>("1"));
		Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>("2"));
		Assert.IsFalse(Testing.LdapService.EntityExists<Dummy>("3"));
		Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>("4"));
		Assert.IsFalse(Testing.LdapService.EntityExists<Dummy>("5"));
	}

	[TestMethod]
	public void TestTryModifyEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true);

		Dummy dummy    = dummies[2];
		Dummy modDummy = dummy.Clone();

		modDummy.HomeDir = "/home/new-home";
		Testing.LdapService.TryModifyEntity(modDummy, dummy.Id.ToString()).AssertSuccess();

		modDummy.Id = 5;
		Testing.LdapService.TryModifyEntity(modDummy, dummy.Id.ToString()).AssertFailure();

		modDummy.Id = 2;
		var result = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString(), true).AssertSuccess();
		Assert.AreEqual(modDummy, result.GetValue());
	}

	[TestMethod]
	public void TestGetDisplayNameOfEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString()).AssertSuccess();

		foreach (Dummy dummy in dummies) {
			string? displayName = Testing.LdapService.TryGetDisplayNameOfEntity(dummy.Id.ToString(), typeof(Dummy));
			Assert.AreEqual(dummy.FullName, displayName);
		}

		Assert.IsNull(Testing.LdapService.TryGetDisplayNameOfEntity("5", typeof(Dummy)));
	}

	[TestMethod]
	public void TestGetPasswordOfEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true).AssertSuccess();

		foreach (Dummy dummy in dummies) {
			string? password = Testing.LdapService.TryGetPasswordOfEntity(dummy.Id.ToString(), typeof(Dummy));
			Assert.AreEqual(dummy.Password, password);
		}

		Assert.IsNull(Testing.LdapService.TryGetPasswordOfEntity("5", typeof(Dummy)));

		UserPassword? adminPass = Authenticator.GetDefaultAdminPasswordAndCrateAdminWhenMissing(Testing.LdapService, out _);
		Assert.IsNotNull(adminPass);
		Assert.IsTrue(adminPass.CheckPassword("adminpass"));

		string? passwordStr = Testing.LdapService.TryGetPasswordOfEntity(Authenticator.GetDefaultAdminName());
		Assert.IsNotNull(passwordStr);
		Assert.IsTrue(new UserPassword(passwordStr).CheckPassword("adminpass"));
	}
}
