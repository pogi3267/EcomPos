using System.Collections.Generic;
using System.Security.Claims;

namespace ApplicationCore.Entities.ApplicationUser
{
    public class UserClaim
    {
        public UserClaim()
        {
            ApplicationClaims = new List<CustomSelectList>();
        }
        public string UserId { get; set; }
        public List<CustomSelectList> ApplicationClaims { get; set; }
    }
    public class CustomSelectList
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public bool Selected { get; set; }
    }
    public static class ClaimStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            new Claim("Create", "Create"),
            new Claim("Edit", "Edit"),
            new Claim("Delete", "Delete"),
            new Claim("Update", "Update"),
        };
    }

}
