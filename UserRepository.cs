using Kairos.Shared.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Kairos.WebAPI.Data.Repositories;

public class UserRepository : IUserRepository
{
	private readonly AppDbContext _dbContext;

	public UserRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<UserEntity> GetUserByIdAsync(long userId)
	{
		return await _dbContext.Users
			.Include(u => u.Roles) 
			.FirstOrDefaultAsync(u => u.Id == userId);
	}

	public async Task<UserEntity> GetUserAsync(string username)
	{
		return await _dbContext.Users
			.Include(u => u.Roles)
			.FirstOrDefaultAsync(u => u.Name == username);
	}
	public async Task<List<UserEntity>> GetAllUsersAsync()
	{
		return await _dbContext.Users
			.Include(u => u.Roles) 
			.ToListAsync();
	}
	public async Task<UserEntity> AddUserAsync(UserEntity input)
	{
		input.Password = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input.Password)));
		_dbContext.Users.Add(input);

		await _dbContext.SaveChangesAsync();
		return input;
	}


	public async Task<UserEntity> UpdateUserAsync(UserEntity input)
	{
		var existingUser = await _dbContext.Users
			.Include(u => u.Roles)
			.FirstOrDefaultAsync(u => u.Id == input.Id);

		if (existingUser == null)
		{
			throw new KeyNotFoundException("User not found");
		}

		if (!string.Equals(existingUser.Name, input.Name))
		{
			existingUser.Name = input.Name;
		}

		if (!string.IsNullOrEmpty(input.Password))
		{
			if (!string.Equals(existingUser.Password, input.Password))
			{
				existingUser.Password = input.Password;
			}
		}

		if (input.Roles != null && input.Roles.Any())
		{
			existingUser.Roles.Clear();
			foreach (var role in input.Roles)
			{
				existingUser.Roles.Add(new RoleEntity { Name = role.Name });
			}
		}

		await _dbContext.SaveChangesAsync();
		return existingUser;
	}


	public async Task<bool> DeleteUserByUsernameAsync(string username)
	{
		var user = await _dbContext.Users
			.Include(u => u.Roles)
			.FirstOrDefaultAsync(u => u.Name == username);

		if (user == null) return false;

		_dbContext.Users.Remove(user);
		await _dbContext.SaveChangesAsync();
		return true;
	}



}
