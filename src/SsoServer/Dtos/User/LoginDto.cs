namespace SsoServer.Dtos.User;

public class LoginDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
}
public class LoginChangePassword : LoginDto
{
    public string NewPassword { get; set; }

}

public class LoginResetPassword
{
    public string UserName { get; set; }

}
public class LoginConfirmResetPassword : LoginDto
{
    public string Code { get; set; }

}