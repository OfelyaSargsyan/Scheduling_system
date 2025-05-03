using Kairos.Shared.Data;

namespace Kairos.WebAPI.Data.Repositories
{
	public interface IUserRepository
	{
		Task<UserEntity> GetUserByIdAsync(long userId);
		Task<UserEntity> GetUserAsync(string username);
		Task<List<UserEntity>> GetAllUsersAsync();
		Task<UserEntity> AddUserAsync(UserEntity input);
		Task<UserEntity> UpdateUserAsync(UserEntity input);
		Task<bool> DeleteUserByUsernameAsync(string username);

	}
}
