using SsoServer.Dtos.User;

namespace SsoServer.Dtos.Client;

public class ClientDto : ClientBaseDto
{
    public List<UserBaseDto> Users { get; set; }
}