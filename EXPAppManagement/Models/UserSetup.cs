using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EXPAppManagement.Models
{
    public class UserSetupModel
    {
        public List<AppUser> AppUser { get; set;}
        public List<Nop_Customer> NopUsers { get; set; }
        public List<SIT_Organisation> NopOrganisations { get; set; }
        public bool showDeleted { get; set; }

        public string screenType { get; set; }
    }
    public class MaintainUserModel
    {
        public bool IsNopUser { get; set; }
        public AppUser appUser { get; set; }
        public Nop_Customer nopUser { get; set; }
        public List<AppUser> allAppUsers { get; set; }
        public List<AppUser> assignedAppUsers { get; set; }
        public List<Nop_Customer> assignedNopUsers { get; set; }
        public List<Nop_Customer> allNopUsers { get; set; }
        public List<AspNetRole> allRoles { get; set; }
    }

    public class MaintainLocationModel
    {
        public bool LocationExists { get; set; }
        public ClockingLocation ExistingLocation { get; set; }
        public List<SIT_Organisation> allNOPOrganisations { get; set; }
    }

}