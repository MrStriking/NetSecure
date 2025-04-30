using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using System.Net.Mail;
using System.Net;
using MailKit.Net.Smtp;
using MimeKit;

namespace NetSecure.Controllers
{
	[Route("forgot-password")]
	public class AccountController : Controller
	{
		private readonly UserManager<IdentityUser> _userManager;

		public AccountController(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		private async Task SendEmailAsync(string email, string subject, string message)
		{
			var emailMessage = new MimeMessage();
			emailMessage.From.Add(new MailboxAddress("NetSecure App", "omarmayassco@gmail.com"));
			emailMessage.To.Add(new MailboxAddress("", email));
			emailMessage.Subject = subject;

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = message
			};
			emailMessage.Body = bodyBuilder.ToMessageBody();

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
				await client.AuthenticateAsync("omarmayassco@gmail.com", "ybst lvfn ithv jvwa");
				await client.SendAsync(emailMessage);
				await client.DisconnectAsync(true);
			}
		}

		[HttpGet("ForgotPassword")]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost("ForgotPassword")]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			if (string.IsNullOrEmpty(email))
				return View();

			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return RedirectToAction("ForgotPasswordConfirmation");

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);
			try
			{
				await SendEmailAsync(
					email,
					"Reset your password",
					$"<h1>Hello!</h1><p>It looks like you requested a password reset.</p><p>Please <a href='{callbackUrl}'>click here</a> to reset your password.</p><p>If you did not request this, you can safely ignore this email.</p>");
			} catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return RedirectToAction("Index","Index");
			}

			return RedirectToAction("ForgotPasswordConfirmation");
		}

		[HttpGet("ForgotPasswordConfirmation")]
		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		[HttpGet("ResetPassword")]
		public IActionResult ResetPassword(string token, string email)
		{
			if (token == null || email == null)
				return BadRequest("Invalid password reset token");

			var model = new ResetPasswordModel { Token = token, Email = email };
			return View(model);
		}

		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return RedirectToAction("ResetPasswordConfirmation");

			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
			if (result.Succeeded)
				return RedirectToAction("ResetPasswordConfirmation");

			foreach (var error in result.Errors)
				ModelState.AddModelError(string.Empty, error.Description);

			return View(model);
		}

		[HttpGet("ResetPasswordConfirmation")]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}
	}

	public class ResetPasswordModel
	{
		public string Email { get; set; }
		public string Token { get; set; }
		public string Password { get; set; }
	}
}
