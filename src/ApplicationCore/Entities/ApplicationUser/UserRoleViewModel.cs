﻿namespace ApplicationCore.Entities.ApplicationUser
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
