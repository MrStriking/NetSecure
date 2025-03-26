using ServiceContracts.DTO;

namespace ServiceContracts
{
	public interface IUsersService
	{
		UserResponse AddUser(UserAddRequest request);

		UserResponse? ValidateUser(LoginRequest request);

		UserResponse? GetUser(string username);

		int GetUserCount();

		bool DeleteUser(string username);

		bool MakeAdmin(string username);

		int GetAdminCount();
	}
}
