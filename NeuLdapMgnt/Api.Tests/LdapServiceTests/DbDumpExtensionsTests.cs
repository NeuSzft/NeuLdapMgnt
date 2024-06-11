namespace NeuLdapMgnt.Api.Tests.LdapServiceTests;

[TestClass]
public class DbDumpExtensionsTests {
	[TestCleanup]
	public void Cleanup() => Testing.EraseLdap();

	[TestMethod]
	public void TestExportDatabase() {
		Testing.LdapService.TryAddEntities(_exampleDump.Students, x => x.Id.ToString(), true).AssertSuccess();
		Testing.LdapService.TryAddEntities(_exampleDump.Employees, x => x.Id, true).AssertSuccess();
		Assert.IsTrue(Testing.LdapService.SetValue(Authenticator.DefaultAdminEnabledValueName, _exampleDump.Values[Authenticator.DefaultAdminEnabledValueName], out _));
		Assert.IsTrue(Testing.LdapService.SetValue(Authenticator.DefaultAdminPasswordValueName, _exampleDump.Values[Authenticator.DefaultAdminPasswordValueName], out _));

		var dump = Testing.LdapService.ExportDatabase().AssertSuccess().Value;
		Assert.IsNotNull(dump);

		CollectionAssert.AreEqual(_exampleDump.Students.ToArray(), dump.Students.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Employees.ToArray(), dump.Employees.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Values, dump.Values);
	}

	[TestMethod]
	public void TestImportDatabase() {
		Testing.LdapService.ImportDatabase(_exampleDump, false).AssertSuccess();

		var dump = Testing.LdapService.ExportDatabase().AssertSuccess().Value;
		Assert.IsNotNull(dump);

		CollectionAssert.AreEqual(_exampleDump.Students.ToArray(), dump.Students.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Employees.ToArray(), dump.Employees.ToArray());
		CollectionAssert.AreEqual(_exampleDump.Values, dump.Values);
	}

	private readonly LdapDbDump _exampleDump = new() {
		Students = [
			new Student {
				Id            = 72000000001,
				Uid           = 6000,
				Gid           = 6000,
				Class         = "10.C",
				Username      = "johdoe",
				FirstName     = "John",
				LastName      = "Doe",
				MiddleName    = null,
				Email         = "john.doe@mail.com",
				HomeDirectory = "/home/johdoe",
				Password      = null,
				FullName      = "John Doe"
			},
			new Student {
				Id            = 72000000002,
				Uid           = 6001,
				Gid           = 6001,
				Class         = "12.A",
				Username      = "jandoe",
				FirstName     = "Jane",
				LastName      = "Doe",
				MiddleName    = null,
				Email         = "jane.doe@mail.com",
				HomeDirectory = "/home/jandoe",
				Password      = null,
				FullName      = "Jane Doe",
				IsInactive    = true
			}
		],
		Employees = [
			new Employee {
				Id            = "george.sears",
				Uid           = 4000,
				Gid           = 4000,
				Class         = "-",
				Username      = "geosea",
				FirstName     = "George",
				LastName      = "Sears",
				MiddleName    = null,
				Email         = "solidus@mail.com",
				HomeDirectory = "/home/geosea",
				Password      = Utils.BCryptHashPassword("lalilulelo"),
				FullName      = "George Sears",
				IsTeacher     = true,
				IsAdmin       = true
			},
		],
		Values = new() {
			{ Authenticator.DefaultAdminEnabledValueName, "False" },
			{ Authenticator.DefaultAdminPasswordValueName, Utils.BCryptHashPassword("adminpass") }
		}
	};
}
