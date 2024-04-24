namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class EntityExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestTryAddEntity() {
		Dummy[] dummies = Dummy.CreateDummies(5).ToArray();
		foreach (Dummy dummy in dummies)
			Testing.LdapService.TryAddEntity(dummy, dummy.UserId.ToString(), true);

		var result = Testing.LdapService.GetAllEntities<Dummy>(true);
		Assert.IsTrue(result.IsSuccess());

		Assert.AreEqual(dummies.Length, result.Values.Length);

		for (int i = 0; i < dummies.Length; i++) {
			Assert.IsTrue(dummies[i].Equals(result.Values[i]));
		}
	}
}
