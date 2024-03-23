using System.Text;

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
				_student.ClassSubGroup = subGroup == 0 ? null : subGroup;
				UpdateClass();
				return this;
			}

			public Builder SetClass(string @class)
			{
				if (@class.Contains('/'))
				{
					string[] classSplit = @class.Split('/', '.', System.StringSplitOptions.TrimEntries);
					_student.ClassYear = int.Parse(classSplit[1]);
					_student.ClassGroup = classSplit[2];
					_student.ClassSubGroup = int.Parse(classSplit[0]);
					UpdateClass();
				}
				else
				{
					string[] classSplit = @class.Split('.', System.StringSplitOptions.TrimEntries);
					_student.ClassYear = int.Parse(classSplit[0]);
					_student.ClassGroup = classSplit[1];
					UpdateClass();
				}

				return this;
			}

			private void UpdateClass()
			{
				StringBuilder builder = new();

				if (_student.ClassSubGroup is not null)
				{
					builder.Append($"{_student.ClassSubGroup}/");
				}
				builder.Append($"{_student.ClassYear}.");
				builder.Append($"{_student.ClassGroup[0].ToString().ToUpper()}{_student.ClassGroup[1..]}");

				_student.Class = builder.ToString();
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