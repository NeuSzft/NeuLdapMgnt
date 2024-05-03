using System.ComponentModel.DataAnnotations;
using NeuLdapMgnt.Models.CustomValidationAttributes;
using NeuLdapMgnt.Models.CustomValidationAttributes.IdAttributes;
using NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes;

namespace NeuLdapMgnt.Models.Tests;

[TestClass]
public class AttributesUnitTests
{
	private ValidationContext _validationContext = default!;

	[TestInitialize]
	public void Initialize()
	{
		_validationContext = new ValidationContext(new object());
	}

	[TestMethod]
	public void StudentIdAttributeValidOmSuccess()
	{
		var attribute = new IdStudentAttribute(Student.IdMinValue, Student.IdMaxValue);
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.IdMinValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.IdMaxValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.IdMinValue + 1, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.IdMaxValue - 1, _validationContext));
	}

	// ID tests
	[TestMethod]
	public void StudentIdAttributeWithInvalidDataTypeFails()
	{
		var attribute = new IdStudentAttribute(Student.IdMinValue, Student.IdMaxValue);
		Assert.AreEqual("ID: Invalid data type",
			attribute.GetValidationResult("string", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void StudentIdAttributeInvalidOmFails()
	{
		var attribute = new IdStudentAttribute(Student.IdMinValue, Student.IdMaxValue);
		Assert.AreEqual($"ID must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
			attribute.GetValidationResult(Student.IdMinValue - 1, _validationContext)!.ErrorMessage);

		Assert.AreEqual($"ID must be between {Student.IdMinValue} and {Student.IdMaxValue}.",
			attribute.GetValidationResult(Student.IdMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherIdAttributeWithInvalidDataTypeFails()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual("ID: Invalid data type",
			attribute.GetValidationResult(123, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherIdAttributeInvalidFormatFails()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual("ID invalid.",
			attribute.GetValidationResult("abc..def", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherIdAttributeIncludesNonDotCharFails()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual("ID must contain only alphanumeric characters or '.'",
			attribute.GetValidationResult("abc,def", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherIdAttributeValidIdSuccess()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("abc.def", _validationContext));
	}

	[TestMethod]
	public void TeacherIdAttributeMissingDotFails()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual("ID must contain '.'",
			attribute.GetValidationResult("abcdef", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherIdAttributeShortPartsFails()
	{
		var attribute = new IdTeacherAttribute();
		Assert.AreEqual("The first part of the ID must be at least 3 characters long.",
			attribute.GetValidationResult("ab.cde", _validationContext)!.ErrorMessage);
		Assert.AreEqual("The second part of the ID must be at least 3 characters long.",
			attribute.GetValidationResult("abc.cd", _validationContext)!.ErrorMessage);
	}

	// User ID tests
	[TestMethod]
	public void UserIdAttributeWithInvalidDataTypeFails()
	{
		var attribute = new UserIdAttribute(Student.UidMinValue, Student.UidMaxValue);
		Assert.AreEqual("User ID: Invalid data type",
			attribute.GetValidationResult("string", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void StudentUidAttributeInvalidUidFails()
	{
		var attribute = new UserIdAttribute(Student.UidMinValue, Student.UidMaxValue);
		Assert.AreEqual($"User ID must be between {Student.UidMinValue} and {Student.UidMaxValue}.",
			attribute.GetValidationResult(Student.UidMinValue - 1, _validationContext)!.ErrorMessage);

		Assert.AreEqual($"User ID must be between {Student.UidMinValue} and {Student.UidMaxValue}.",
			attribute.GetValidationResult(Student.UidMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void StudentUidAttributeValidUidSuccess()
	{
		var attribute = new UserIdAttribute(Student.UidMinValue, Student.UidMaxValue);
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.UidMinValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.UidMinValue + 1, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.UidMaxValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.UidMaxValue - 1, _validationContext));
	}

	[TestMethod]
	public void TeacherUidAttributeValidUidSuccess()
	{
		var attribute = new UserIdAttribute(Teacher.UidMinValue, Teacher.UidMaxValue);
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Teacher.UidMinValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Teacher.UidMaxValue, _validationContext));
	}

	[TestMethod]
	public void TeacherUidAttributeInvalidUidFails()
	{
		var attribute = new UserIdAttribute(Teacher.UidMinValue, Teacher.UidMaxValue);
		Assert.AreEqual($"User ID must be between {Teacher.UidMinValue} and {Teacher.UidMaxValue}.",
			attribute.GetValidationResult(Teacher.UidMinValue - 1, _validationContext)!.ErrorMessage);

		Assert.AreEqual($"User ID must be between {Teacher.UidMinValue} and {Teacher.UidMaxValue}.",
			attribute.GetValidationResult(Teacher.UidMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	// Group ID tests
	[TestMethod]
	public void GroupIdAttributeWithInvalidDataTypeFails()
	{
		var attribute = new GroupIdAttribute(Student.GidMinValue, Student.GidMaxValue);
		Assert.AreEqual("Group ID: Invalid data type",
			attribute.GetValidationResult("string", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void StudentGidAttributeValidGidSuccess()
	{
		var attribute = new GroupIdAttribute(Student.GidMinValue, Student.GidMaxValue);
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.GidMinValue, _validationContext));


		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Student.GidMaxValue, _validationContext));


		Assert.AreEqual($"Group ID must be between {Student.GidMinValue} and {Student.GidMaxValue}.",
			attribute.GetValidationResult(Student.GidMinValue - 1, _validationContext)!.ErrorMessage);

		Assert.AreEqual($"Group ID must be between {Student.GidMinValue} and {Student.GidMaxValue}.",
			attribute.GetValidationResult(Student.GidMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void StudentGidAttributeInvalidGidFails()
	{
		var attribute = new GroupIdAttribute(Student.GidMinValue, Student.GidMaxValue);
		Assert.AreEqual($"Group ID must be between {Student.GidMinValue} and {Student.GidMaxValue}.",
			attribute.GetValidationResult(Student.GidMinValue - 1, _validationContext)!.ErrorMessage);

		Assert.AreEqual($"Group ID must be between {Student.GidMinValue} and {Student.GidMaxValue}.",
			attribute.GetValidationResult(Student.GidMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void TeacherGidAttributeValidGidSuccess()
	{
		var attribute = new GroupIdAttribute(Teacher.GidMinValue, Teacher.GidMaxValue);
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Teacher.GidMinValue, _validationContext));

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(Teacher.GidMaxValue, _validationContext));
	}

	[TestMethod]
	public void TeacherGidAttributeInvalidGidFails()
	{
		var attribute = new GroupIdAttribute(Teacher.GidMinValue, Teacher.GidMaxValue);
		Assert.AreEqual($"Group ID must be between {Teacher.GidMinValue} and {Teacher.GidMaxValue}.",
			attribute.GetValidationResult(Teacher.GidMinValue - 1, _validationContext)!.ErrorMessage);
		Assert.AreEqual($"Group ID must be between {Teacher.GidMinValue} and {Teacher.GidMaxValue}.",
			attribute.GetValidationResult(Teacher.GidMaxValue + 1, _validationContext)!.ErrorMessage);
	}

	// FirstName tests
	[TestMethod]
	public void FirstNameAttributeValidSuccess()
	{
		var attribute = new FirstNameAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("John", _validationContext));
	}

	[TestMethod]
	public void FirstNameAttributeShortFails()
	{
		var attribute = new FirstNameAttribute();
		Assert.AreEqual("First name must be at least 3 characters long.",
			attribute.GetValidationResult("Jo", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void FirstNameAttributeWithInvalidDataTypeFails()
	{
		var attribute = new FirstNameAttribute();
		Assert.AreEqual("First name: Invalid data type",
			attribute.GetValidationResult(123, _validationContext)!.ErrorMessage);
	}

	// LastName tests
	[TestMethod]
	public void LastNameAttributeValidSuccess()
	{
		var attribute = new LastNameAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("Doe", _validationContext));
	}

	[TestMethod]
	public void LastNameAttributeShortFails()
	{
		var attribute = new LastNameAttribute();

		Assert.AreEqual("Last name must be at least 3 characters long.",
			attribute.GetValidationResult("Do", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void LastNameAttributeWithInvalidDataTypeFails()
	{
		var attribute = new LastNameAttribute();

		Assert.AreEqual("Last name: Invalid data type",
			attribute.GetValidationResult(123, _validationContext)!.ErrorMessage);
	}

	// MiddleName tests
	[TestMethod]
	public void MiddleNameAttributeNullSuccess()
	{
		var attribute = new MiddleNameAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(null, _validationContext));
	}

	[TestMethod]
	public void MiddleNameAttributeValidSuccess()
	{
		var attribute = new MiddleNameAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("Smith", _validationContext));
	}

	[TestMethod]
	public void MiddleNameAttributeShortFails()
	{
		var attribute = new MiddleNameAttribute();

		Assert.AreEqual("Middle name must be at least 3 characters long.",
			attribute.GetValidationResult("Sm", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void MiddleNameAttributeWithInvalidDataTypeFails()
	{
		var attribute = new MiddleNameAttribute();

		Assert.AreEqual("Middle name: Invalid data type",
			attribute.GetValidationResult(123, _validationContext)!.ErrorMessage);
	}

	// Email tests
	[TestMethod]
	public void EmailAttributeValidEmailSuccess()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("example@example", _validationContext));

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("example@example.com", _validationContext));
	}

	[TestMethod]
	public void EmailAttributeEmptyEmailSuccess()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("", _validationContext));
	}

	[TestMethod]
	public void EmailAttributeInvalidEmailNoAtSignFails()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual("Email is not a valid email address.",
			attribute.GetValidationResult("example.com", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void EmailAttributeEmailWithMultipleAtSignsFails()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual("Email is not a valid email address.",
			attribute.GetValidationResult("example@@example.com", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void EmailAttributeEmailWithSpecialCharacterFails()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual("Email must contain only alphanumeric characters '.', '@'.",
			attribute.GetValidationResult("example@example.com!", _validationContext)!.ErrorMessage);

		Assert.AreEqual("Email must contain only alphanumeric characters '.', '@'.",
			attribute.GetValidationResult("examp*le@example.com", _validationContext)!.ErrorMessage);
	}
	
	[TestMethod]
	public void EmailAttributeNullSuccess()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(null, _validationContext));
	}
	
	[TestMethod]
	public void EmailAttributeInvalidDataTypeFails()
	{
		var attribute = new EmailAttribute();

		Assert.AreEqual("Email: Invalid data type",
			attribute.GetValidationResult(int.MaxValue, _validationContext)!.ErrorMessage);
	}
	
	// Directory tests
	[TestMethod]
	public void DirectoryAttributeValidDirectorySuccess()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("/home/username", _validationContext));
	}

	[TestMethod]
	public void DirectoryAttributeInvalidStartFails()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual("Directory must start with '/home/'.",
			attribute.GetValidationResult("/usr/local", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void DirectoryAttributeInvalidEndFails()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual("Directory must not end with '/'.",
			attribute.GetValidationResult("/home/", _validationContext)!.ErrorMessage);

		Assert.AreEqual("Directory must not end with '/'.",
			attribute.GetValidationResult("/home/username/", _validationContext)!.ErrorMessage);

		Assert.AreEqual("Directory must not end with '/'.",
			attribute.GetValidationResult("/home/username//", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void DirectoryAttributeContainsSpacesSuccess()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("/home/user name", _validationContext));
	}

	[TestMethod]
	public void DirectoryAttributeCaseInsensitiveCheckSuccess()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("/HOME/username", _validationContext));
	}

	[TestMethod]
	public void DirectoryAttributeWithInvalidDataTypeFails()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual("Directory: Invalid data type",
			attribute.GetValidationResult(12345, _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void DirectoryAttributeSpecialCharactersSuccess()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("/home/user@name", _validationContext));
	}

	[TestMethod]
	public void DirectoryAttributeMultipleSubdirectoriesSuccess()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("/home/username/subdir/subsubdir", _validationContext));
	}

	[TestMethod]
	public void DirectoryAttributeContainsHomeNotAtStartFails()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual("Directory must start with '/home/'.",
			attribute.GetValidationResult("usr/home/username", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void DirectoryAttributeEndsWithHomeFails()
	{
		var attribute = new DirectoryAttribute();
		Assert.AreEqual("Directory must not end with '/'.",
			attribute.GetValidationResult("/home/username/home/", _validationContext)!.ErrorMessage);
	}

	// Password tests
	[TestMethod]
	public void PasswordAttributeTooShortFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password must be at least 8 characters long.",
			attribute.GetValidationResult("Abc!123", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void PasswordAttributeMissingLowercaseFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password must contain at least one lowercase letter.",
			attribute.GetValidationResult("ABC123!@", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void PasswordAttributeMissingUppercaseFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password must contain at least one uppercase letter.",
			attribute.GetValidationResult("abc123!@", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void PasswordAttributeMissingDigitFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password must contain at least one number.",
			attribute.GetValidationResult("Abcdef!@", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void PasswordAttributeMissingSpecialCharacterFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password must contain at least one special character.",
			attribute.GetValidationResult("Abc123de", _validationContext)!.ErrorMessage);
	}

	[TestMethod]
	public void PasswordAttributeValidPasswordSuccess()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult("Abc123!@", _validationContext));
	}

	[TestMethod]
	public void PasswordAttributeWithInvalidDataTypeFails()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual("Password: Invalid data type",
			attribute.GetValidationResult(12345678, _validationContext)!.ErrorMessage);
	}
	
	[TestMethod]
	public void PasswordAttributeEmptySuccess()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(string.Empty, _validationContext));
	}
	
	[TestMethod]
	public void PasswordAttributeNullSuccess()
	{
		var attribute = new PasswordAttribute();
		Assert.AreEqual(ValidationResult.Success,
			attribute.GetValidationResult(null, _validationContext));
	}
}
