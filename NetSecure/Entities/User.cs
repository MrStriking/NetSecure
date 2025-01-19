using System.ComponentModel.DataAnnotations;

namespace Entities
{
	public class User
	{
		[Key]
		public string? Username { get; set; }
		public string? Password { get; set; }
		[Key]
		[EmailAddress]
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
	}
}
