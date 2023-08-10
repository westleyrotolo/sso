namespace SsoServer.Dtos.User;

public class UserDto
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Id { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Clients { get; set; }

}