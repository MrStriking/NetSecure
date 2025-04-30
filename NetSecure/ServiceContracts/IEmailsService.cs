using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
	public interface IEmailsService
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
}
