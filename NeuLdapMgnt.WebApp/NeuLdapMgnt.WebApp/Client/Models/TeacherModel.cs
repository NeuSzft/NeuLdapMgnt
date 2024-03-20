using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.WebApp.Client.Models
{
	public class TeacherModel
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required, Phone]
		public string Phone { get; set; }

		public TeacherModel(int id, string firstName, string lastName, string phone)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
			Phone = phone;
		}
	}
}
