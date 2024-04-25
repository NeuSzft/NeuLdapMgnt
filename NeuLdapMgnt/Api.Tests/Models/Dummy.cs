namespace NeuLdapMgnt.Api.Tests.Models;

[LdapObjectClasses("inetOrgPerson", "posixAccount")]
public sealed class Dummy : ICloneable, IEquatable<Dummy> {
	[LdapAttribute("uid")] public int Id { get; set; }

	[LdapAttribute("uidNumber")] public int UserId { get; set; }

	[LdapAttribute("gidNumber")] public int GroupId { get; set; }

	[LdapAttribute("sn")] public string FirstName { get; set; } = string.Empty;

	[LdapAttribute("cn")] public string LastName { get; set; } = string.Empty;

	[LdapAttribute("displayName")] public string FullName { get; set; } = string.Empty;

	[LdapAttribute("homeDirectory")] public string HomeDir { get; set; } = string.Empty;

	[LdapAttribute("userPassword", true)] public string? Password { get; set; }

	[LdapAttribute("roomNumber")] public string? RoomNumber { get; set; }

	object ICloneable.Clone() => Clone();

	public Dummy Clone() {
		return new() {
			Id = Id,
			UserId = UserId,
			GroupId = GroupId,
			FirstName = FirstName,
			LastName = LastName,
			FullName = FullName,
			HomeDir = HomeDir,
			Password = Password,
			RoomNumber = RoomNumber
		};
	}

	public bool Equals(Dummy? other) {
		return other is not null
		       && Id == other.Id
		       && UserId == other.UserId
		       && GroupId == other.GroupId
		       && FirstName == other.FirstName
		       && LastName == other.LastName
		       && FullName == other.FullName
		       && HomeDir == other.HomeDir
		       && Password == other.Password
		       && RoomNumber == other.RoomNumber;
	}

	public override bool Equals(object? obj) {
		return obj is Dummy dummy && Equals(dummy);
	}

	public override string ToString() {
		return $"[{Id}] {UserId}:{GroupId} {FullName} '{HomeDir}' {Password}";
	}

	public static IEnumerable<Dummy> CreateDummies(int n) {
		for (int i = 0; i < n; i++)
			yield return new Dummy {
				Id = i,
				UserId = 1000 + i,
				GroupId = 1000 + i,
				FirstName = "Test",
				LastName = $"Dummy{i:D2}",
				FullName = $"Test Dummy{i:D2}",
				HomeDir = $"/home/dummy.{i:D2}",
				Password = $"password{i:D2}"
			};
	}
}
