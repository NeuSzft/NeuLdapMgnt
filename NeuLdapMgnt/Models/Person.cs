﻿using NeuLdapMgnt.Models.CustomValidationAttributes;
using NeuLdapMgnt.Models.CustomValidationAttributes.NameAttributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
    [LdapObjectClasses("inetOrgPerson", "posixAccount")]
    public abstract class Person
    {
        private string _surname = string.Empty;
        private string _givenName = string.Empty;
        private string? _middleName;

        private string _homeDirectory = string.Empty;

        private string _email = string.Empty;

        [Required(ErrorMessage = "User ID is required.")]
        [JsonRequired, JsonPropertyName("uid")]
        [LdapAttribute("uidNumber")]
        public abstract int Uid { get; set; }

        [Required(ErrorMessage = "Group ID is required.")]
        [JsonRequired, JsonPropertyName("gid")]
        [LdapAttribute("gidNumber")]
        public abstract int Gid { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [JsonRequired, JsonPropertyName("username")]
        [LdapAttribute("cn")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Given name is required.")]
        [GivenName]
        [JsonRequired, JsonPropertyName("first_name")]
        [LdapAttribute("givenName")]
        public string GivenName
        {
            get => _givenName;
            set
            {
                _givenName = value.Trim();
                FullName = GetFullName();
                Username = GetUsername();
                HomeDirectory = GetHomeDirectory();
            }
        }

        [Required(ErrorMessage = "Surname is required.")]
        [Surname]
        [JsonRequired, JsonPropertyName("last_name")]
        [LdapAttribute("sn")]
        public string Surname
        {
            get => _surname;
            set
            {
                _surname = value.Trim();
                FullName = GetFullName();
                Username = GetUsername();
                HomeDirectory = GetHomeDirectory();
            }
        }

        [MiddleName]
        [JsonInclude, JsonPropertyName("middle_name")]
        [LdapAttribute("title")]
        public string? MiddleName
        {
            get => _middleName;
            set
            {
                _middleName = NullIfEmptyOrWhitespace(value);
                FullName = GetFullName();
            }
        }

        [Email]
        [JsonRequired, JsonPropertyName("email")]
        [LdapAttribute("mail")]
        public string? Email
        {
            get => _email;
            set
            {
                if (value is null) return;
                _email = RemoveDiacritics(value).ToLower();
            }
        }

        [Required(ErrorMessage = "Directory is required.")]
        [Directory]
        [JsonRequired, JsonPropertyName("home_directory")]
        [LdapAttribute("homeDirectory")]
        public string HomeDirectory
        {
            get => _homeDirectory;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _homeDirectory = "/home/";
                    return;
                }
                _homeDirectory = value.Replace(" ", "");
            }
        }

        [Password]
        [PasswordPropertyText(true)]
        [JsonInclude, JsonPropertyName("password")]
        [LdapAttribute("userPassword", true)]
        public virtual string? Password { get; set; }

        [JsonPropertyName("full_name")]
        [LdapAttribute("displayName")]
        public string FullName { get; set; } = string.Empty;

        [JsonInclude, JsonPropertyName("is_inactive")]
        [LdapFlag("inactive")]
        public bool IsInactive { get; set; }

        private string GetFullName()
        {
            StringBuilder builder = new();
            TextInfo textInfo = new CultureInfo("hu-HU", false).TextInfo;
            string capitalizedFirstName = textInfo.ToTitleCase(GivenName);
            string capitalizedMiddleName = string.IsNullOrEmpty(MiddleName) ? string.Empty : textInfo.ToTitleCase(MiddleName);
            string capitalizedLastName = textInfo.ToTitleCase(Surname);

            if (string.IsNullOrEmpty(capitalizedMiddleName))
            {
                builder.Append(capitalizedFirstName + ' ');
                builder.Append(capitalizedLastName);
            }
            else
            {
                builder.Append(capitalizedFirstName + ' ');
                builder.Append(capitalizedLastName + ' ');
                builder.Append(capitalizedMiddleName);
            }

            return builder.ToString().Trim();
        }

        public string GetUsername()
        {
            return RemoveDiacritics($"{GivenName.PadRight(3, '_')[..3]}{Surname.PadRight(3, '_')[..3]}").ToLower();
        }

        private string GetHomeDirectory()
        {
            return string.IsNullOrEmpty(Username) ? "/home/" : $"/home/{GetUsername()}";
        }

        protected static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        protected string? NullIfEmptyOrWhitespace(string? str)
        {
            if (str is null)
                return null;
            str = str.Trim();
            return str.Length == 0 ? null : str;
        }
    }
}
