namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class GroupExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestSetAndGetMembersOfGroup() {
		string[] members = ["0", "1", "2", "3", "4"];
		Assert.IsTrue(Testing.LdapService.SetMembersOfGroup("dummies", members));

		Assert.IsTrue(Testing.LdapService.GroupExists("dummies"));
		CollectionAssert.AreEqual(members, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());

		members = ["0", "1", "2", "4", "8"];
		Assert.IsTrue(Testing.LdapService.SetMembersOfGroup("dummies", members));

		Assert.IsTrue(Testing.LdapService.GroupExists("dummies"));
		CollectionAssert.AreEqual(members, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());
	}

	[TestMethod]
	public void TestSetMembersOfGroupToEmpty() {
		Assert.IsTrue(Testing.LdapService.SetMembersOfGroup("dummies", []));

		Assert.IsTrue(Testing.LdapService.GroupExists("dummies"));
		Assert.IsFalse(Testing.LdapService.GetMembersOfGroup("dummies").Any());
	}

	[TestMethod]
	public void TestTryAddAndRemoveGroup() {
		Testing.LdapService.TryAddGroup("dummies").AssertSuccess();
		Assert.IsTrue(Testing.LdapService.GroupExists("dummies"));

		Testing.LdapService.TryAddGroup("dummies").AssertFailure();

		Testing.LdapService.TryDeleteGroup("dummies").AssertSuccess();
		Assert.IsFalse(Testing.LdapService.GroupExists("dummies"));
	}

	[TestMethod]
	public void TestTryAddEntityToGroup() {
		Testing.LdapService.TryAddGroup("dummies").AssertSuccess();

		string[] ids = ["0", "1", "2", "3", "4"];
		foreach (var id in ids)
			Testing.LdapService.TryAddEntityToGroup("dummies", id).AssertSuccess();
		Testing.LdapService.TryAddEntityToGroup("dummies", ids[2]).AssertFailure();

		foreach (var id in ids)
			Assert.IsTrue(Testing.LdapService.PartOfGroup("dummies", id));
		Assert.IsFalse(Testing.LdapService.PartOfGroup("dummies", "6"));

		CollectionAssert.AreEqual(ids, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());
	}

	[TestMethod]
	public void TestTryAddEntitiesToGroup() {
		Testing.LdapService.TryAddGroup("dummies").AssertSuccess();

		string[] ids = ["0", "1", "2", "3", "4"];
		Testing.LdapService.TryAddEntitiesToGroup("dummies", ids.Take(3)).AssertSuccess();
		Testing.LdapService.TryAddEntitiesToGroup("dummies", ids.TakeLast(4)).AssertFailure().AssertErrors(2);

		foreach (var id in ids)
			Assert.IsTrue(Testing.LdapService.PartOfGroup("dummies", id));
		Assert.IsFalse(Testing.LdapService.PartOfGroup("dummies", "6"));

		CollectionAssert.AreEqual(ids, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());
	}

	[TestMethod]
	public void TestTryRemoveEntityFromGroup() {
		Testing.LdapService.TryAddGroup("dummies").AssertSuccess();

		Testing.LdapService.TryAddEntitiesToGroup("dummies", ["0", "1", "2", "3", "4"]).AssertSuccess();

		Testing.LdapService.TryRemoveEntityFromGroup("dummies", "0").AssertSuccess();
		Testing.LdapService.TryRemoveEntityFromGroup("dummies", "3").AssertSuccess();
		Testing.LdapService.TryRemoveEntityFromGroup("dummies", "0").AssertFailure();
		Testing.LdapService.TryRemoveEntityFromGroup("dummies", "5").AssertFailure();

		CollectionAssert.AreEqual(new[] { "1", "2", "4" }, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());
	}

	[TestMethod]
	public void TestTryRemoveEntitiesFromGroup() {
		Testing.LdapService.TryAddGroup("dummies").AssertSuccess();

		Testing.LdapService.TryAddEntitiesToGroup("dummies", ["0", "1", "2", "3", "4"]).AssertSuccess();

		Testing.LdapService.TryRemoveEntitiesFromGroup("dummies", ["0", "3", "0", "5"]).AssertErrors(2);

		CollectionAssert.AreEqual(new[] { "1", "2", "4" }, Testing.LdapService.GetMembersOfGroup("dummies").ToArray());
	}
}
