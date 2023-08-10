using SsoServer.Models;
using SsoServer.Models.Users;

namespace SsoServer.Services;

public interface IUserService
{
    Task<ApplicationUser> AddUserAsync(ApplicationUser user, string password, params string[] roles);
    Task<ApplicationUser> UpdateUserAsync(ApplicationUser user, params string[] roles);
    Task ChangePasswordAsync(string userName, string currentPassword, string newPassword);
    Task<IList<ApplicationUser>> GetAllUserAsync();
    Task<IList<ApplicationUser>> GetAllUserByClientIdAsync(string clientId);
    Task DeleteUserAsync(string userName);
    Task AssociateUserToClientId(string userName, string clientId);
    Task RemoveUserFromClientAsync(string userName, string clientId);
}