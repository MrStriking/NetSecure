// Services/EmailService.cs
using Microsoft.Extensions.Configuration;
using MimeKit;
using ServiceContracts;
using System.Net;
//using System.Net.Mail;
using MailKit.Net.Smtp;
using MimeKit;

namespace Services
{
	public class EmailsService : IEmailsService
	{
		public async Task SendEmailAsync(string email, string subject, string message)
		{
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(new MailboxAddress("NetSecure", "omarmayassco@gmail.com"));
			emailMessage.To.Add(new MailboxAddress("", email));
			emailMessage.Subject = subject;
			emailMessage.Body = new TextPart("html") { Text = message };

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
				await client.AuthenticateAsync("omarmayassco@gmail.com", "dfxi fatn megv maon");
				await client.SendAsync(emailMessage);
				await client.DisconnectAsync(true);
			}
		}
	}

}
