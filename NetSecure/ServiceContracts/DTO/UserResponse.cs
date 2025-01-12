using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
	public class UserResponse
	{
		public String? Username { get; set; }
		public String? Password { get; set; }
		public EmailAddressAttribute? Email { get; set; }
		public String? PhoneNumber { get; set; }
	}

	public static class UserExtension
	{
		public static UserResponse ToUserResponse(this User user)
		{
			return new UserResponse { Username = user.Username, Password = user.Password, Email = user.Email, PhoneNumber = user.PhoneNumber };
		}
	}
}
