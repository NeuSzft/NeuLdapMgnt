using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NeuLdapMgnt.Models
{
	public class Teacher : Person
	{
        [Required, RegularExpression("^7[0-9]{10}$")]
        [JsonPropertyName("id")]
        [LdapAttribute("uid")]
        public string Id { get; set; }

		[Required, Range(4000, 5999)]
		[LdapAttribute("uid")]
		public override int Uid { get; set; } = 4000;

		[Required, Range(4000, 5999)]
		[LdapAttribute("gidNumber")]
		public override int Gid { get; set; } = 4000;
	}
}
