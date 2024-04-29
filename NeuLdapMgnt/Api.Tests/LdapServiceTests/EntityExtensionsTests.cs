namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class EntityExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestPasswordChecking() {
		const string pwd  = "example-password";
		string       hash = Utils.BCryptHashPassword(pwd);
		Assert.IsTrue(Utils.CheckBCryptPassword(hash, pwd));
	}

	[TestMethod]
	public void TestTryAddEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		foreach (Dummy dummy in dummies)
			Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertSuccess();

		foreach (var dummy in dummies)
			Assert.IsTrue(Testing.LdapService.EntityExists<Dummy>(dummy.Id.ToString()));
	}

	[TestMethod]
	public void TestTryAddAndGetEntityHiddenAttribute() {
		Dummy dummy = Dummy.CreateDummies(1).First();

		Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true).AssertSuccess();

		Dummy? withHidden = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString(), true).AssertSuccess().GetValue();
		Assert.IsNotNull(withHidden);
		Assert.IsNotNull(withHidden.Password);

		Dummy? withoutHidden = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString()).AssertSuccess().GetValue();
		Assert.IsNotNull(withoutHidden);
		Assert.IsNull(withoutHidden.Password);
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

		CollectionAssert.AreEqual(dummies, result.Values);
	}

	[TestMethod]
	public void TestTryAddAndGetEntitiesOverwrite() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true).AssertSuccess();

		foreach (var dummy in dummies)
			dummy.GroupId = 1000;

		Testing.LdapService.TryAddEntities(dummies, x => x.Id.ToString(), true, true).AssertSuccess();

		var result = Testing.LdapService.GetAllEntities<Dummy>(true).AssertSuccess();
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
		Dummy dummy = Dummy.CreateDummies(1).First();
		Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true);

		Dummy modDummy = dummy.Clone();

		modDummy.HomeDir = "/home/new-home";
		Testing.LdapService.TryModifyEntity(modDummy, dummy.Id.ToString()).AssertSuccess();

		modDummy.Id = 1;
		Testing.LdapService.TryModifyEntity(modDummy, dummy.Id.ToString()).AssertFailure();

		modDummy.Id = 0;
		var result = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString(), true).AssertSuccess();
		Assert.AreEqual(modDummy, result.GetValue());
	}

	[TestMethod]
	public void TestTryModifyEntityNullableAttribute() {
		Dummy dummy = Dummy.CreateDummies(1).First();
		Testing.LdapService.TryAddEntity(dummy, dummy.Id.ToString(), true);

		Dummy modDummy = dummy.Clone();
		modDummy.RoomNumber = null;

		Testing.LdapService.TryModifyEntity(modDummy, dummy.Id.ToString()).AssertSuccess();

		var request = Testing.LdapService.TryGetEntity<Dummy>(dummy.Id.ToString(), true).AssertSuccess();
		Assert.AreEqual(modDummy, request.GetValue());
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

		string? hashBase64 = Authenticator.GetDefaultAdminPasswordAndCrateAdminWhenMissing(Testing.LdapService, out var error);
		Assert.IsNull(error);
		Assert.IsNotNull(hashBase64);
		Assert.IsTrue(Utils.CheckBCryptPassword(hashBase64, "adminpass"));

		hashBase64 = Testing.LdapService.TryGetPasswordOfEntity(Authenticator.GetDefaultAdminName());
		Assert.IsNotNull(hashBase64);
		Assert.IsTrue(Utils.CheckBCryptPassword(hashBase64, "adminpass"));
	}
}
