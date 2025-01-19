using ServiceContracts.DTO;

namespace ServiceContracts
{
	public interface IUsersService
	{
		UserResponse AddUser(UserAddRequest request);

		UserResponse? ValidateUser(LoginRequest request);
	}
}
