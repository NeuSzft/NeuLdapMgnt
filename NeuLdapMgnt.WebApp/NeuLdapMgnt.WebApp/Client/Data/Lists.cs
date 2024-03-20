using NeuLdapMgnt.WebApp.Client.Models;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp.Client.Data
{
	public static class Lists
	{
		private static List<Student> Students { get; set; } = new()
		{
			new ("73358355233", 6000, 6000, "Valamilyen", "Pistike", "11.a", "Husveti"),
			new ("72654768165", 6022, 6022, "Akármilyen", "Huculu", "12.b"),
			new ("68419681654", 6343, 6343, "Tóth", "Muculu", "13.c"),
			new ("72981654865", 6434, 6434, "Valamilyen", "Valaki", "11.d"),
			new ("79816541685", 9341, 9341, "Valaki", "Ismeretlen", "9.a/1"),
			new ("79816541685", 6721, 6721, "Asus", "Matrica", "10.a/2"),
			new ("79816541685", 7256, 7256, "Intel", "Alaplap", "10.b/1", "I9"),
			new ("79816541685", 6112, 6112, "Amd", "Processzor", "10.a"),
			new ("79816541685", 7132, 7132, "Sok", "Ram", "10.b/2")
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

		public static List<Student> GetStudents() => Students;
		public static List<TeacherModel> GetTeachers() => Teachers;
		public static List<AdminModel> GetAdmins() => Admins;
	}
}
