using Entities;
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
			User user = request.ToUser();
			_db.Users.Add(user);
			_db.SaveChanges();
			return user.ToUserResponse();
		}
	}
}
