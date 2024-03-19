namespace NeuLdapMgnt.WebApp.Client.Models
{
	public class StudentModel
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int ClassNumber { get; set; }
		public string ClassSymbol { get; set; }

		public StudentModel(int id, string firstName, string lastName, int classNumber, string classSymbol)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
			ClassNumber = classNumber;
			ClassSymbol = classSymbol;
		}
	}
}
