using NeuLdapMgnt.WebApp.Model;
using NeuLdapMgnt.Models;

namespace NeuLdapMgnt.WebApp
{
	public static class DatabaseLocal
	{
		public static List<Student> Students { get; set; } = new();

		public static List<Teacher> Teachers { get; set; } = new();

		public static List<string> Admins { get; set; } = new();
	}
}
