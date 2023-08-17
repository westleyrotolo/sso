// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SsoServer.Models.Users
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        [NotMapped]
        public List<string> Clients { get; set; }
        [NotMapped]
        public List<string> Roles { get; set; }
    }
}