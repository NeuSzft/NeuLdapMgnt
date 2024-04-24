namespace NeuLdapMgnt.Api.Tests.Models;

[LdapObjectClasses("account", "posixAccount")]
public sealed class Dummy : IEquatable<Dummy> {
	[LdapAttribute("uid")] public int Id { get; init; }

	[LdapAttribute("uidNumber")] public int UserId { get; init; }

	[LdapAttribute("gidNumber")] public int GroupId { get; init; }

	[LdapAttribute("cn")] public string Name { get; init; } = string.Empty;

	[LdapAttribute("homeDirectory")] public string HomeDir { get; init; } = string.Empty;

	[LdapAttribute("userPassword", true)] public string? Password { get; init; }

	public bool Equals(Dummy? other) {
		return other is not null
		       && Id == other.Id
		       && UserId == other.UserId
		       && GroupId == other.GroupId
		       && Name == other.Name
		       && HomeDir == other.HomeDir
		       && Password == other.Password;
	}

	public override bool Equals(object? obj) {
		return obj is Dummy dummy && Equals(dummy);
	}

	public override int GetHashCode() {
		return HashCode.Combine(Id, UserId, GroupId, Name, HomeDir, Password);
	}

	public override string ToString() {
		return $"[{Id}] {UserId}:{GroupId} {Name} '{HomeDir}' {Password}";
	}

	public static IEnumerable<Dummy> CreateDummies(int n) {
		for (int i = 0; i < n; i++)
			yield return new Dummy {
				Id = i,
				UserId = 1000 + i,
				GroupId = 1000 + i,
				Name = $"dummy.{i:D2}",
				HomeDir = $"/home/dummy.{i:D2}",
				Password = $"password{i:D2}"
			};
	}
}
