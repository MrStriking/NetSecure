using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ServiceContracts.DTO
{
	public class UserAddRequest
	{
		[Required(ErrorMessage = "Username can't be blank")]
		public string? Username { get; set; }

		[Required(ErrorMessage = "Password can't be blank")]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
		public string? Password { get; set; }

		[Required(ErrorMessage = "Email can't be blank")]
		[EmailAddress]
		public string? Email { get; set; }

		[Required(ErrorMessage = "Phone Number can't be blank")]
		public string? PhoneNumber { get; set; }

		public User ToUser()
		{
			return new User { Username = Username, Password = Password, Email = Email?.ToLower(), PhoneNumber = PhoneNumber };
		}
	}
}
