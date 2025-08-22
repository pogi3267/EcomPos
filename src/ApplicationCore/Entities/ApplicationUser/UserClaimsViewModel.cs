using System.Collections.Generic;

namespace ApplicationCore.Entities.ApplicationUser
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Cliams = new List<CustomSelectList>();
        }

        public string UserId { get; set; }
        public List<CustomSelectList> Cliams { get; set; }
    }


}
