using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using System.Security.Cryptography;

namespace Services
{
	public class UsersService : IUsersService
	{
		private readonly UserDbContext _db;

		public UsersService(UserDbContext userDbContext)
		{
			_db = userDbContext;
		}

		public UserResponse AddUser(UserAddRequest request)
		{
			bool usernameExists = _db.Users.Any(u => u.Username == request.Username);
			if (usernameExists)
			{
				throw new Exception("The username is already taken.");
			}

			bool emailExists = _db.Users.Any(u => u.Email == request.Email);
			if (emailExists)
			{
				throw new Exception("The email address is already taken.");
			}
			request.Password = SimpleHash(request.Password);
			User user = request.ToUser();
			user.SelectedLab = null;
			user.IsAdmin = false;
			_db.Users.Add(user);
			_db.SaveChanges();
			return user.ToUserResponse();
		}
		public static string SimpleHash(string password)
		{
			byte[] salt = RandomNumberGenerator.GetBytes(16); // 128-bit salt
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
			return Convert.ToBase64String(salt) + "|" + Convert.ToBase64String(hash);
		}
		public static bool Verify(string hashedPassword, string inputPassword)
		{
			var parts = hashedPassword.Split('|');
			byte[] salt = Convert.FromBase64String(parts[0]);
			byte[] storedHash = Convert.FromBase64String(parts[1]);

			var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, 100000, HashAlgorithmName.SHA256);
			byte[] inputHash = pbkdf2.GetBytes(32);

			return CryptographicOperations.FixedTimeEquals(storedHash, inputHash);
		}

		public UserResponse? ValidateUser(LoginRequest request)
		{
			var user = _db.Users.FirstOrDefault(u => u.Username == request.Username.ToLower());
			if (user == null)
			{
				return null;
			}
			if (!Verify(user.Password, request.Password))
			{
				return null;
			}
			return user.ToUserResponse();
		}

		public UserResponse? GetUser(string username)
		{
			if (string.IsNullOrEmpty(username)) { return null; }
			User? user = _db.Users.FirstOrDefault(temp=>temp.Username == username.ToLower());
			if (user == null) { return null; }
			return user.ToUserResponse();
		}

		public int GetUserCount() { 
			return _db.Users.Count(); 
		}

		public bool DeleteUser(string username)
		{
			User? user = _db.Users.FirstOrDefault(u => u.Username == username.ToLower());
			if (user == null)
			{
				return false; 
			}
			_db.Users.Remove(user);
			_db.SaveChanges();
			return true;
		}

		public bool MakeAdmin(string username)
		{
			User? user = _db.Users.FirstOrDefault(u => u.Username == username.ToLower());
			if (user == null)
			{
				return false;
			}
			user.IsAdmin = true;
			_db.SaveChanges();
			return true;
		}

		public int GetAdminCount()
		{
			return _db.Users.Count(u => u.IsAdmin == true);
		}
	}
}
