using NeuLdapMgnt.WebApp.Client.Models;
using NeuLdapMgnt.Models;
using NeuLdapMgnt.Models.Factory;

namespace NeuLdapMgnt.WebApp.Client.Data
{
	public static class Lists
	{
		public static List<Student> Students { get; set; } = new();

		private static List<Teacher> Teachers { get; set; } = new()
		{
			//new ("Tanar vagyok", 5000, 5000, "Elso", "Pistike", "Kozep"),
			//new ("Tanar vagyok", 5424, 5424, "Harmat", "Huculu"),
			//new ("Tanar vagyok", 5452, 5452, "Tóth", "Laci"),
			//new ("Tanar vagyok", 5100, 5100, "Akarmilyen", "Valaki", "Hihi"),
			//new ("Tanar vagyok", 5041, 5041, "Valaki", "Ismeretlen", "Aki")
		};

		private static List<AdminModel> Admins { get; set; } = new()
		{
			new (1001, "KreativNev", "Elso", "Pistike"),
			new (1005, "Fihookfq", "Tóth", "Laci"),
			new (1006, "SAiuhguqw", "Akarmilyen", "Valaki"),
			new (1003, "gfsiunbqe", "Harmat", "Huculu"),
			new (1010, "OKoiuhnasd", "Valaki", "Ismeretlen")
		};

		public static List<Student> GetStudents() => Students;
		public static List<Teacher> GetTeachers() => Teachers;
		public static List<AdminModel> GetAdmins() => Admins;
		public static Random? random { get; set; }
		public static void GenerateStudents(int count)
		{
			random = new();
			for (int i = 0; i < count; i++)
			{
				Student student = StudentFactory
					.CreateEmptyStudent()
					.SetId(70000000000 + i)
					.SetUid(6000 + Students.Count)
					.SetGid(6000 + Students.Count)
					.SetUsername($"User{i}")
					.SetFirstName("Kis")
					.SetMiddleName("Pista")
					.SetLastName($"Ifjabbik {i}.")
					.SetPassword($"password{i}")
					.SetEmail($"{i}@example.com")
					.SetClass(
						random.Next(Student.AllowedYearsRange.Min, Student.AllowedYearsRange.Max + 1),
						Student.AllowedGroups[random.Next(Student.AllowedGroups.Length)],
						random.Next(3))
					.SetHomeDirectory($"/home/{i}")
					.Build();

				Students.Add(student);
			}
		}
	}
}
