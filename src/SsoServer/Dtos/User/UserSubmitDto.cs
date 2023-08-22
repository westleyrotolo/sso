namespace SsoServer.Dtos.User;

public class UserSubmitDto
{

    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Id { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Clients { get; set; }
    public string Qualification { get; set; }
    public string FiscalCode { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string PhoneNumber { get; set; }
    public string MobilePhoneNumber { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string Password { get; set; }
}