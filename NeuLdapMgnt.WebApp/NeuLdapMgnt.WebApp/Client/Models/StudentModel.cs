using System.ComponentModel.DataAnnotations;

namespace NeuLdapMgnt.WebApp.Client.Models
{
	public class StudentModel
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public string OM { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public int ClassNumber { get; set; }

		[Required]
		public string ClassSymbol { get; set; }

		public StudentModel(int id, string oM, string firstName, string lastName, int classNumber, string classSymbol)
		{
			Id = id;
			OM = oM;
			FirstName = firstName;
			LastName = lastName;
			ClassNumber = classNumber;
			ClassSymbol = classSymbol;
		}
	}
}
