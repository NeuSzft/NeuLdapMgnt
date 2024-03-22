namespace NeuLdapMgnt.Models.Factory
{
	public static class StudentFactory
	{
		public static Builder CreateEmptyStudent()
		{
			return new Builder(new Student());
		}

		public class Builder
		{
			private readonly Student _student;

			public Builder(Student student)
			{
				_student = student;
			}

			public Builder SetId(long id)
			{
				_student.Id = id;
				return this;
			}

			public Builder SetFirstName(string firstName)
			{
				_student.FirstName = firstName;
				return this;
			}

			public Builder SetMiddleName(string? middleName)
			{
				_student.MiddleName = middleName;
				return this;
			}

			public Builder SetLastName(string lastName)
			{
				_student.LastName = lastName;
				return this;
			}

			public Builder SetClass(int year, string group, int? subGroup = null)
			{
				_student.ClassYear = year;
				_student.ClassGroup = group;
				_student.ClassSubGroup = subGroup;
				return this;
			}

			public Builder SetUsername(string username)
			{
				_student.Username = username;
				return this;
			}

			public Builder SetUid(int uid)
			{
				_student.Uid = uid;
				return this;
			}

			public Builder SetGid(int gid)
			{
				_student.Gid = gid;
				return this;
			}

			public Builder SetEmail(string email)
			{
				_student.Email = email;
				return this;
			}

			public Builder SetHomeDirectory(string homeDirectory)
			{
				_student.HomeDirectory = homeDirectory;
				return this;
			}

			public Builder SetPassword(string password)
			{
				_student.Password = password;
				return this;
			}

			public Student Build()
			{
				return _student;
			}
		}
	}
}