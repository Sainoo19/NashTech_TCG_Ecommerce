﻿namespace NashTech_TCG_RazorPages.Models.Responses
{
    public class RolesResponse
    {
        public IEnumerable<string> Roles { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
