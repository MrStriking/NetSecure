using System.ComponentModel.DataAnnotations;

namespace Entities
{
	public class User
	{
		[Key]
		public String? Username { get; set; }
		public String? Password { get; set; }
		[Key]
		public EmailAddressAttribute? Email { get; set; }
		public String? PhoneNumber { get; set; }
	}
}
