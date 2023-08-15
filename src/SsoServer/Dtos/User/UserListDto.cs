using System;
namespace SsoServer.Dtos.User
{
	public class UserListDto
	{
		public string ClientId { get; set; }
		public List<UserItemListDto> Users { get; set; }
	}
	public class UserItemListDto
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public List<string> Roles { get; set; }
	}
}

