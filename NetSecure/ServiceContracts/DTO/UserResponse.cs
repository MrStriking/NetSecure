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
		public string? Username { get; set; }
		public string? Password { get; set; }
		[EmailAddress]
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
	}

	public static class UserExtension
	{
		public static UserResponse ToUserResponse(this User user)
		{
			return new UserResponse { Username = user.Username, Password = user.Password, Email = user.Email?.ToLower(), PhoneNumber = user.PhoneNumber };
		}
	}
}
