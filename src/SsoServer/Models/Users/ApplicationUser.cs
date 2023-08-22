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
            CreatedAt = DateTime.Now;
        }
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public string Qualification { get; set; }
        public string FiscalCode { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string MobilePhoneNumber { get; set; }
        [NotMapped]
        public List<string> Clients { get; set; }
        [NotMapped]
        public List<string> Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}