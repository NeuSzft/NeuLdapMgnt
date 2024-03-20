using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.WebApp.Client.Models
{
	public class AdminModel
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		public AdminModel(int id, string username, string firstName, string lastName)
		{
			Id = id;
			Username = username;
			FirstName = firstName;
			LastName = lastName;
		}
	}
}
