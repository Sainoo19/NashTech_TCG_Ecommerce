using System;
using System.Collections.Generic;

namespace NashTech_TCG_API.Models.DTOs
{
    public class RolesResponse
    {
        public IList<string> Roles { get; set; } = new List<string>();
        public string Email { get; set; }
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RedirectUrl { get; set; }
    }
}
