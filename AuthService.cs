using Kairos.Shared.Data;
using Kairos.WebAPI.Data.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Kairos.WebAPI.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LoginResponse> LoginAsync(UserEntity data)
    {
        var res = await _userRepository.GetUserAsync(data.Name);

        if (res is null)
        {
            return new LoginResponse
            {
                Code = -1
            };
        }

        if (Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data.Password))) != res.Password)
        {
            return new LoginResponse
            {
                Code = -2
            };
        }

        return new LoginResponse
        {
            Code = 0,
            UserName = res.Name,
            UserId = res.Id,
            Roles = res.Roles.Select(r => r.Name).ToList(),
        };
    }
}

