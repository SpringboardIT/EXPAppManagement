using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EXPAppManagement.Models
{
    public class ClockingModel
    {
        public Guid? SiteID { get; set; }
        public Guid? EmployeeID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<AppUser> allUsers { get; set; }
        public List<ClockingLocation> allLocations { get; set; }
        public List<Timesheet> Timesheets { get; set; }

        public bool ShowApproved { get; set; }
        public bool ShowUnApproved { get; set; }



        public class AllocateTimeModel
        {
            public Guid UniqueID { get; set; }
            public int Duration { get; set; }
        }
        public static class DateHelper
        {
            public static DateTime RoundUp(DateTime dt, TimeSpan d)
            {
                var modTicks = dt.Ticks % d.Ticks;
                var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
                return new DateTime(dt.Ticks + delta, dt.Kind);
            }

            public static DateTime RoundDown(DateTime? dt, TimeSpan d)
            {
                var delta = dt.Value.Ticks % d.Ticks;
                return new DateTime(dt.Value.Ticks - delta, dt.Value.Kind);
            }
            public static DateTime RoundToNearest(DateTime? dt, TimeSpan d)
            {
                if (dt == null)
                {
                    return DateTime.MinValue;
                }
                if (dt == DateTime.MaxValue || dt == DateTime.MinValue)
                {
                    return dt.Value;
                }

                var delta = dt.Value.Ticks % d.Ticks;
                bool roundUp = delta > d.Ticks / 2;
                var offset = roundUp ? d.Ticks : 0;

                return new DateTime(dt.Value.Ticks + offset - delta, dt.Value.Kind);
            }
        }
    }
}