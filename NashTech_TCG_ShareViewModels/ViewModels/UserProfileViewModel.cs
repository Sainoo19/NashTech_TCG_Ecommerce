using System;
using System.Collections.Generic;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
    }
}
