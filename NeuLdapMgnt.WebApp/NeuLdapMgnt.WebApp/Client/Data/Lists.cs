using NeuLdapMgnt.WebApp.Client.Models;

namespace NeuLdapMgnt.WebApp.Client.Data
{
	public static class Lists
	{
		public static List<StudentModel> Students { get; private set; } = new()
		{
			new (1, "Valamilyen", "Pistike", 11, "a"),
			new (2, "Akármilyen", "Huculu", 12, "b"),
			new (3, "Tóth", "Muculu", 13, "c"),
			new (4, "Valamilyen", "Valaki", 11, "d"),
			new (5, "Valaki", "Ismeretlen", 9, "a")
		};

		public static List<StudentModel> GetStudents() => Students;
	}
}
