namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class DbDumpExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestExportDatabase() {
		Testing.LdapService.TryAddEntities(_exampleDump.Students, x => x.Id.ToString(), true).AssertSuccess();
		Testing.LdapService.TryAddEntities(_exampleDump.Teachers, x => x.Id, true).AssertSuccess();
		Testing.LdapService.TryAddEntityToGroup("admin", _exampleDump.Teachers.First().Id).AssertSuccess();
		Testing.LdapService.TryAddEntityToGroup("inactive", _exampleDump.Students.Last().Id.ToString()).AssertSuccess();
		Assert.IsTrue(Testing.LdapService.SetValue(Authenticator.DefaultAdminEnabledValueName, _exampleDump.Values[Authenticator.DefaultAdminEnabledValueName], out _));
		Assert.IsTrue(Testing.LdapService.SetValue(Authenticator.DefaultAdminPasswordValueName, _exampleDump.Values[Authenticator.DefaultAdminPasswordValueName], out _));

		var dump = Testing.LdapService.ExportDatabase().AssertSuccess().GetValue();
		Assert.IsNotNull(dump);

		CollectionAssert.AreEqual(_exampleDump.Students.ToArray(), dump.Students.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Teachers.ToArray(), dump.Teachers.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Admins.ToArray(), dump.Admins.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Inactives.ToArray(), dump.Inactives.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Values, dump.Values);
	}

	[TestMethod]
	public void TestImportDatabase() {
		Testing.LdapService.ImportDatabase(_exampleDump, false).AssertSuccess();

		var dump = Testing.LdapService.ExportDatabase().AssertSuccess().GetValue();
		Assert.IsNotNull(dump);

		CollectionAssert.AreEqual(_exampleDump.Students.ToArray(), dump.Students.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Teachers.ToArray(), dump.Teachers.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Admins.ToArray(), dump.Admins.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Inactives.ToArray(), dump.Inactives.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Values, dump.Values);
	}

	readonly LdapDbDump _exampleDump = new() {
		Students = [
			new Student {
				Id = 72000000001,
				Uid = 6000,
				Gid = 6000,
				Class = "10.C",
				Username = "johdoe",
				FirstName = "John",
				LastName = "Doe",
				MiddleName = null,
				Email = "john.doe@mail.com",
				HomeDirectory = "/home/johdoe",
				Password = "planTextPassword",
				FullName = "John Doe"
			},
			new Student {
				Id = 72000000002,
				Uid = 6001,
				Gid = 6001,
				Class = "12.A",
				Username = "jandoe",
				FirstName = "Jane",
				LastName = "Doe",
				MiddleName = null,
				Email = "jane.doe@mail.com",
				HomeDirectory = "/home/jandoe",
				Password = "planTextPassword",
				FullName = "Jane Doe"
			}
		],
		Teachers = [
			new Employee {
				Id = "george.sears",
				Uid = 4000,
				Gid = 4000,
				Class = "-",
				Username = "geosea",
				FirstName = "George",
				LastName = "Sears",
				MiddleName = "",
				Email = "solidus@mail.com",
				HomeDirectory = "/home/geosea",
				Password = "planTextPassword",
				FullName = "George Sears"
			},
		],
		Admins = [
			"george.sears"
		],
		Inactives = [
			"72000000002"
		],
		Values = new() {
			{ Authenticator.DefaultAdminEnabledValueName, "False" },
			{ Authenticator.DefaultAdminPasswordValueName, "planTextPassword" }
		}
	};
}
