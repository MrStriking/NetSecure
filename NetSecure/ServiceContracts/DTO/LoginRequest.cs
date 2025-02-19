using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "Username can't be blank")]
		public string? Username { get; set; }

		[Required(ErrorMessage = "Password can't be blank")]
		public string? Password { get; set; }

		[Required(ErrorMessage = "IP can't be blank")]
		public string? IP { get; set; }

	}
}
