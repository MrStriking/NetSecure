using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

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

			User user = request.ToUser();
			user.SelectedLevel = null;
			_db.Users.Add(user);
			_db.SaveChanges();
			return user.ToUserResponse();
		}

		public UserResponse? ValidateUser(LoginRequest request)
		{
			var user = _db.Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
			if (user == null)
			{
				return null;
			}
			return user.ToUserResponse();
		}

		public UserResponse? GetUser(string username)
		{
			User? user = _db.Users.FirstOrDefault(temp=>temp.Username == username);
			if (user == null) { return null; }
			return user.ToUserResponse();
		}
	}
}
