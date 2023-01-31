using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EXPAppManagement.Models
{
    public class LocationSetupModel
    {
        public List<ClockingLocation> Locations { get; set; }
        public List<SIT_Organisation> NopOrganisations { get; set; }
    }
    public class LocationModel
    {
        public Guid LocationID { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public SIT_Organisation Organisation { get; set; }
        public int Tolerance { get; set; }
    }
}