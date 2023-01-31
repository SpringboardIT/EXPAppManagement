using EXPAppManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class ClockingController : ApiController
    {
        public ClockingLocations Get(Guid AccessToken, Guid Secret)
        {

            ClockingLocations locations = new ClockingLocations();
            locations.Locations = new List<ClockingLocation>();
            if (APIHelper.IsAuthenticated(AccessToken, Secret))
            {
                using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                {
                    foreach (var loc in context.ClockingLocations.ToList())
                    {
                        ClockingLocation nLoc = new ClockingLocation();
                        nLoc.ID = loc.ID;
                        nLoc.Latitude = loc.Latitude;
                        nLoc.Longitude = loc.Longitude;
                        nLoc.Location = loc.LocationName;
                        nLoc.Tolerance = loc.Tolerance;
                        locations.Locations.Add(nLoc);
                    }
                }
            }
            return locations;
        }
        public ClockingData Put(Guid AccessToken, Guid Secret, [FromBody] ClockingData clockingData)
        {
            if (APIHelper.IsAuthenticated(AccessToken, Secret))
            {
                using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                {
                    AppUser user = context.AppUsers.Where(x => x.AccessToken == AccessToken && x.Secret == Secret).FirstOrDefault();
                    foreach (Clocking clocking in clockingData.ClockingLines)
                    {
                        Timesheet nTimesheet = null;
                        bool IsNew = false;
                        Guid uniqueID = Guid.NewGuid();
                        if (clocking.UniqueID == new Guid()) // new Record!!!!!!!!!!!
                        {
                            nTimesheet = new Timesheet();
                            IsNew = true;
                        }
                        else
                        {
                            uniqueID = clocking.UniqueID;
                            nTimesheet = context.Timesheets.FirstOrDefault(x => x.UniqueID == clocking.UniqueID);
                            if (nTimesheet == null) //header no longer exists!
                            {
                                nTimesheet = new Timesheet();
                                uniqueID = Guid.NewGuid();
                            }
                        }
                        nTimesheet.ClockingLocationID = clocking.SiteID;
                            if (clocking.ClockIn.HasValue && nTimesheet.ClockInDateTime == DateTime.MinValue)
                            {
                                nTimesheet.ClockInDateTime = clocking.ClockIn.Value;
                                //nTimesheet.ClockInLat = clocking.ClockInLat;
                                //nTimesheet.ClockInLong = clocking.ClockInLong;
                                //nTimesheet.OutOfRangeClockInJustification = clocking.OutOfRangeClockInReason;

                            }
                        if (clocking.ClockOut.HasValue && nTimesheet.ClockOutDateTime == null)
                        {
                            nTimesheet.ClockOutDateTime = clocking.ClockOut.Value;
                            //nTimesheet.ClockOutLat = clocking.ClockOutLat;
                            //nTimesheet.ClockOutLong = clocking.ClockOutLong;
                            //nTimesheet.OutOfRangeClockOutJustification = clocking.OutOfRangeClockOutReason;
                        }
                        //nTimesheet.UserEmailAddress = clocking.EmailAddress;
                        nTimesheet.UniqueID = uniqueID;
                        if (user != null)
                        {
                            nTimesheet.AppUserID = user.ID;
                        }
                        clocking.UniqueID = uniqueID; //only really needed for New but not a bad thing to be explicit.


                        if (clocking.ClockIn.HasValue)
                        {

                            //dont loo k up by timsheet id, instead does timesheet have clockin ID set? 
                            ClockingTransaction inTrans = context.ClockingTransactions.FirstOrDefault(x => x.TimesheetID == uniqueID && x.ClockType == 1);
                            if (inTrans == null)
                            {
                             
                                inTrans = new ClockingTransaction();
                                inTrans.ClockType = 1;
                                inTrans.DateTime = clocking.ClockIn.Value;
                                inTrans.Justification = clocking.OutOfRangeClockInReason;
                                inTrans.Latitude = clocking.ClockInLat;
                                inTrans.Longitude = clocking.ClockInLong;
                                inTrans.ClockingLocationID = clocking.SiteID;
                                //inTrans.UserEmailAddress = clocking.EmailAddress;
                                //inTrans.TimesheetID = uniqueID;

                                if (user != null)
                                {
                                    inTrans.AppUserID = user.ID;
                                }
                                    inTrans.UniqueID = Guid.NewGuid();
                                nTimesheet.ClockInTransaction = inTrans.UniqueID;
                                context.ClockingTransactions.Add(inTrans);
                            }

                         
                            
                        }
                        if (clocking.ClockOut.HasValue)
                        {
                            //dont loo k up by timsheet id, instead does timesheet have clockout ID set? 
                            ClockingTransaction outTrans = context.ClockingTransactions.FirstOrDefault(x => x.TimesheetID == uniqueID && x.ClockType == 2);
                            if (outTrans == null)
                            {
                                
                                outTrans = new ClockingTransaction();
                                outTrans.ClockType = 2;
                                outTrans.DateTime = clocking.ClockOut.Value;
                                outTrans.Justification = clocking.OutOfRangeClockOutReason;
                                outTrans.Latitude = clocking.ClockOutLat;
                                outTrans.Longitude = clocking.ClockOutLong;
                                outTrans.ClockingLocationID = clocking.SiteID;
                                //outTrans.TimesheetID = uniqueID;
                                //outTrans.UserEmailAddress = clocking.EmailAddress;
                                if (user != null)
                                {
                                    outTrans.AppUserID = user.ID;
                                }
                                 
                                outTrans.UniqueID = Guid.NewGuid();
                                nTimesheet.ClockOutTransaction = outTrans.UniqueID;
                                context.ClockingTransactions.Add(outTrans);
                            }
            
                           
                        }
                        if (IsNew)
                        {
                            context.Timesheets.Add(nTimesheet);
                        }

                    }
                    context.SaveChanges();
                }
            }
            else
            {
                throw new Exception("User Unauthorised");
            }
            return clockingData;
        }
        public class ClockingLocations
        {
            public List<ClockingLocation> Locations { get; set; }
        }
        public class ClockingLocation
        {
            public Guid ID { get; set; }
            public string Location { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int Tolerance { get; set; }
        }

        public class ClockingData
        {
            public List<Clocking> ClockingLines { get; set; }
        }


        public class Clocking
        {
            public int liID { get; set; }
            public Guid UniqueID { get; set; }
            public string EmailAddress { get; set; }
            public double ClockInLat { get; set; }
            public double ClockInLong { get; set; }
            public double ClockOutLat { get; set; }
            public double ClockOutLong { get; set; }
            public DateTime? ClockIn { get; set; }
            public DateTime? ClockOut { get; set; }
            public DateTime? LastUpdated { get; set; }
            public string OutOfRangeClockInReason { get; set; }
            public string OutOfRangeClockOutReason { get; set; }
            public Guid? SiteID { get; set; }
        }

    }
}