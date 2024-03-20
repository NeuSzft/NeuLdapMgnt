using NeuLdapMgnt.WebApp.Client.Models;

namespace NeuLdapMgnt.WebApp.Client.Data
{
	public static class Lists
	{
		private static List<StudentModel> Students { get; set; } = new()
		{
			new (6021, "73358355233", "Valamilyen", "Pistike", 11, "a"),
			new (6022, "72654768165", "Akármilyen", "Huculu", 12, "b"),
			new (6343, "68419681654", "Tóth", "Muculu", 13, "c"),
			new (6034, "72981654865", "Valamilyen", "Valaki", 11, "d"),
			new (6341, "79816541685", "Valaki", "Ismeretlen", 9, "a/1"),
			new (8721, "79816541685", "Asus", "Matrica", 10, "a/2"),
			new (7256, "79816541685", "Intel", "Alaplap", 10, "b/1"),
			new (6112, "79816541685", "Amd", "Processzor", 10, "a"),
			new (7132, "79816541685", "Sok", "Ram", 10, "b/2")
		};

		private static List<TeacherModel> Teachers { get; set; } = new()
		{
			new (5000, "Elso", "Pistike", "+3612345678"),
			new (5424, "Harmat", "Huculu", "+3601235874"),
			new (5452, "Tóth", "Laci", "+3666666666"),
			new (5100, "Akarmilyen", "Valaki","+3609825874"),
			new (5041, "Valaki", "Ismeretlen", "+3647898749")
		};

		private static List<AdminModel> Admins { get; set; } = new()
		{
			new (1001, "KreativNev", "Elso", "Pistike"),
			new (1005, "Fihookfq", "Tóth", "Laci"),
			new (1006, "SAiuhguqw", "Akarmilyen", "Valaki"),
			new (1003, "gfsiunbqe", "Harmat", "Huculu"),
			new (1010, "OKoiuhnasd", "Valaki", "Ismeretlen")
		};

		public static List<StudentModel> GetStudents() => Students;
		public static List<TeacherModel> GetTeachers() => Teachers;
		public static List<AdminModel> GetAdmins() => Admins;
	}
}
