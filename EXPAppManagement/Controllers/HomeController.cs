using EXPAppManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EXPAppManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var userStore = new UserStore<IdentityUser>();
            var manager = new UserManager<IdentityUser>(userStore);

            return View();
        }

        public ActionResult UserSetup(string Type, bool ShowDeleted = false)
        {
            UserSetupModel model = new UserSetupModel();
            model.AppUser = new List<AppUser>();
            model.NopUsers = new List<Nop_Customer>();
            model.showDeleted = ShowDeleted;
            model.screenType = Type;
            if (Type.ToUpper() == "NOP")
            {
                using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
                {
                    List<int> allOrgs = context.SIT_Organisation.Where(x => x.Deleted == false).Select(x => x.OrganisationID).ToList();
                    model.NopUsers = context.Nop_Customer.Where(x => x.SITOrganisationID.HasValue).Where(x => x.Deleted == false && x.Active == true && allOrgs.Contains(x.SITOrganisationID.Value)).ToList();
                    model.NopOrganisations = context.SIT_Organisation.ToList();
                }
            }
            else
            {
                using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                {
                    model.AppUser = context.AppUsers.ToList();
                }
            }
            return View(model);
        }

        public ActionResult AddUserMapping(Guid AppUserID, int NopUserID)
        {
            using (ExpAppClockingsEntities appContext = new ExpAppClockingsEntities())
            {
                DataMappingType dMapType = appContext.DataMappingTypes.FirstOrDefault(x => x.eNumMapping == (int)eDataMappingTypes.UserToNopAccount);
                List<DataMapping> dMaps = appContext.DataMappings.Where(x => x.DataMappingType == dMapType.ID && x.SecondaryID == NopUserID.ToString() && x.PrimaryID == AppUserID).ToList();
                if (dMaps.Count == 0)
                {
                    DataMapping dMap = new DataMapping();
                    dMap.ID = Guid.NewGuid();
                    dMap.PrimaryID = AppUserID;
                    dMap.SecondaryID = NopUserID.ToString();
                    dMap.DataMappingType = dMapType.ID;
                    appContext.DataMappings.Add(dMap);
                    appContext.SaveChanges();
                }
            }
            return PartialView();
        }

        public ActionResult RemoveUserMapping(Guid AppUserID, int NopUserID)
        {
            using (ExpAppClockingsEntities appContext = new ExpAppClockingsEntities())
            {
                DataMappingType dMapType = appContext.DataMappingTypes.FirstOrDefault(x => x.eNumMapping == (int)eDataMappingTypes.UserToNopAccount);
                List<DataMapping> dMaps = appContext.DataMappings.Where(x => x.DataMappingType == dMapType.ID && x.SecondaryID == NopUserID.ToString() && x.PrimaryID == AppUserID).ToList();
                if (dMaps.Count > 0)
                {
                    foreach (DataMapping dMap in dMaps)
                    {
                        appContext.DataMappings.Remove(dMap);
                    }

                }
                appContext.SaveChanges();
            }
            return PartialView();
        }

        public ActionResult MaintainLocation(string ID)
        {
            MaintainLocationModel model = new MaintainLocationModel();
            using (ExpediteTestPortalEntities nopContext = new ExpediteTestPortalEntities())
            {
                using (ExpAppClockingsEntities appContext = new ExpAppClockingsEntities())
                {
                    model.LocationExists = false;
                    model.allNOPOrganisations = nopContext.SIT_Organisation.Where(x => x.Deleted == false).ToList();
                    if (!string.IsNullOrEmpty(ID))
                    {
                        var loc = appContext.ClockingLocations.Where(x => x.ID == new Guid(ID)).FirstOrDefault();
                        if (loc != null)
                        {
                            model.ExistingLocation = loc;
                            model.LocationExists = true;
                        }
                    }
                }
            }
            return View(model);
        }
        public ActionResult DeleteUser(string ID, bool IsNopUser)
        {
            Guid ExistingID = new Guid(ID);
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                AppUser aUser = context.AppUsers.FirstOrDefault(x => x.ID == ExistingID);
                if (aUser != null)
                {
                    aUser.Deleted = true;
                    context.SaveChanges();
                }
                return RedirectToAction("UserSetup", "Home", new { Type = "APP" });
            }
        }

        public ActionResult UnDeleteUser(string ID, bool IsNopUser)
        {
            Guid ExistingID = new Guid(ID);
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                AppUser aUser = context.AppUsers.FirstOrDefault(x => x.ID == ExistingID);
                if (aUser != null)
                {
                    aUser.Deleted = false;
                    context.SaveChanges();
                }
                return RedirectToAction("UserSetup", "Home", new { Type = "APP" });
            }
        }

        public ActionResult MaintainUser(string ID, bool IsNopUser)
        {
            //Reading in from the database and filling page with content
            MaintainUserModel model = new MaintainUserModel();
            model.IsNopUser = IsNopUser;

            using (ExpediteTestPortalEntities nopContext = new ExpediteTestPortalEntities())
            {
                using (ExpAppClockingsEntities appContext = new ExpAppClockingsEntities())
                {
                    List<int> allOrgs = nopContext.SIT_Organisation.Where(x => x.Deleted == false).Select(x => x.OrganisationID).ToList();

                    DataMappingType dMapType = appContext.DataMappingTypes.FirstOrDefault(x => x.eNumMapping == (int)eDataMappingTypes.UserToNopAccount);
                    if (IsNopUser)
                    {
                        int liID = 0;
                        int.TryParse(ID, out liID);
                        model.nopUser = nopContext.Nop_Customer.FirstOrDefault(x => x.CustomerID == liID);
                        model.assignedAppUsers = new List<AppUser>();
                        if (model.nopUser != null)
                        {
                            List<DataMapping> dMaps = appContext.DataMappings.Where(x => x.DataMappingType == dMapType.ID && x.SecondaryID == ID).ToList();
                            model.allAppUsers = appContext.AppUsers.ToList();
                            foreach (AppUser aUser in model.allAppUsers)
                            {
                                if (dMaps.FirstOrDefault(x => x.PrimaryID == aUser.ID) != null)
                                {
                                    model.assignedAppUsers.Add(aUser);
                                }
                            }
                        }

                    }
                    else
                    {
                        Guid lgID = new Guid();
                        Guid.TryParse(ID, out lgID);
                        model.appUser = appContext.AppUsers.Include("AspNetRoles").FirstOrDefault(x => x.ID == lgID);
                        
                        model.assignedNopUsers = new List<Nop_Customer>();
                        if (model.appUser != null)
                        {
                            List<DataMapping> dMaps = appContext.DataMappings.Where(x => x.DataMappingType == dMapType.ID && x.PrimaryID == lgID).ToList();
                            model.allNopUsers = nopContext.Nop_Customer.Where(x => x.SITOrganisationID.HasValue).Where(x => x.Deleted == false && x.Active == true && x.Username != string.Empty && allOrgs.Contains(x.SITOrganisationID.Value)).ToList();
                            foreach (Nop_Customer aUser in model.allNopUsers)
                            {
                                if (dMaps.FirstOrDefault(x => x.SecondaryID == aUser.CustomerID.ToString()) != null)
                                {
                                    model.assignedNopUsers.Add(aUser);
                                }
                            }
                            model.allRoles = appContext.AspNetRoles.ToList();
                        }
                    }
                }
            }
            //all roles
            //existing user
            //all users nop/app
            return View(model);
        }

        public ActionResult AllocateTime(Guid UniqueID)
        {
            ClockingModel.AllocateTimeModel model = new ClockingModel.AllocateTimeModel();
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                Timesheet tSheet = context.Timesheets.FirstOrDefault(x => x.UniqueID == UniqueID);

                if (tSheet != null)
                {
                    if (!tSheet.Duration.HasValue)
                    {
                        TimeSpan ts = new TimeSpan(0); if (!tSheet.ClockOutDateTime.HasValue) { ts = new TimeSpan(0); } else { ts = tSheet.ClockOutDateTime.Value - tSheet.ClockInDateTime; }
                        int RoundedDuration = (int)ts.TotalMinutes % 15 >= 7 ? (int)ts.TotalMinutes + 15 - (int)ts.TotalMinutes % 15 : (int)ts.TotalMinutes - (int)ts.TotalMinutes % 15;
                        model.Duration = RoundedDuration;
                    }
                    else
                    {
                        model.Duration = tSheet.Duration.Value;
                    }
                    model.UniqueID = UniqueID;
                    
                }
            }
            return PartialView("partAllocateTime", model);
        }

        public ActionResult InsertClocking(FormCollection form)
        {
            Guid liEmpID = SITMVCFormHelper.GetGuidFromForm(form, "EmployeeID");
            Guid liSiteID = SITMVCFormHelper.GetGuidFromForm(form, "SiteID");
            DateTime liDate = SITMVCFormHelper.GetDateTimeFromForm(form, "Date");
            int liDuration = SITMVCFormHelper.GetIntegerFromForm(form, "Duration");
            string lsComments = SITMVCFormHelper.GetStringFromForm(form, "Comments");
            bool lbApproved = SITMVCFormHelper.GetBooleanFromForm(form, "Approved");

           ///DbInterception.Add(new TemporalTableCommandTreeInterceptor());
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                Timesheet tSheet = new Timesheet();
                tSheet.UniqueID = Guid.NewGuid();
                tSheet.AppUserID = liEmpID;
                tSheet.ClockingLocationID = liSiteID;
                tSheet.ClockInDateTime = liDate;
                tSheet.Duration = liDuration;
                tSheet.Comments = lsComments;
                tSheet.Approved = lbApproved;
                if (lbApproved)
                {
                    string liUsername = User.Identity.GetUserName();
                    AspNetUser user = context.AspNetUsers.FirstOrDefault(x => x.UserName == liUsername);
                    if (user != null)
                    {
                        tSheet.ApprovedBy = new Guid(user.Id);
                    }
                }
                context.Timesheets.Add(tSheet);
                context.SaveChanges();
            }
            return RedirectToAction("Clockings", "Home");

        }
        public ActionResult SaveClockings(FormCollection form)
        {
            List<string> processedIDs = new List<string>();
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                foreach (string key in form.Keys)
                {
                    if (key != "Timesheets_length")
                    {
                        string lsTimesheetID = key.Split(new string[] { "--" }, StringSplitOptions.None)[1];
                        if (!processedIDs.Contains(lsTimesheetID))
                        {
                            Guid liSiteID = SITMVCFormHelper.GetGuidFromForm(form, "site--" + lsTimesheetID);
                            DateTime liDate = SITMVCFormHelper.GetDateTimeFromForm(form, "date--" + lsTimesheetID);
                            int liDuration = SITMVCFormHelper.GetIntegerFromForm(form, "duration--" + lsTimesheetID);
                            string lsComments = SITMVCFormHelper.GetStringFromForm(form, "comments--" + lsTimesheetID);
                            bool lbApproved = SITMVCFormHelper.GetBooleanFromForm(form, "approved--" + lsTimesheetID);

                            Timesheet tSheet = context.Timesheets.FirstOrDefault(x => x.UniqueID == new Guid(lsTimesheetID));
                            if (tSheet != null)
                            {
                                if (liSiteID != new Guid())
                                {
                                    tSheet.ClockingLocationID = liSiteID;
                                }
                                else
                                {
                                    tSheet.ClockingLocationID = null;
                                }
                               
                                tSheet.Duration = liDuration;
                                tSheet.Comments = lsComments;
                                tSheet.Approved = lbApproved;
                                if (lbApproved)
                                {
                                    string liUsername = User.Identity.GetUserName();
                                    AspNetUser user = context.AspNetUsers.FirstOrDefault(x => x.UserName == liUsername);
                                    if (user != null)
                                    {
                                        tSheet.ApprovedBy = new Guid(user.Id);
                                    }
                                }
                            }


                            processedIDs.Add(lsTimesheetID);
                        }
                    }

                }
                context.SaveChanges();
            }
            return RedirectToAction("Clockings", "Home");
        }
        public ActionResult SaveUser(FormCollection form)
            {
            //Get the info from the new controls and save to database
                bool IsNopUser = false;
                bool.TryParse(SITMVCFormHelper.GetStringFromForm(form, "IsNopUser"), out IsNopUser);
                if (!IsNopUser)
                {
                    Guid ExistingID = SITMVCFormHelper.GetGuidFromForm(form, "UserID");
                    using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                    {
                        AppUser aUser = context.AppUsers.FirstOrDefault(x => x.ID == ExistingID);
                        List<AspNetRole> allRoles = context.AspNetRoles.ToList();
                        if (aUser != null)
                        {
                            aUser.AspNetRoles.Clear();
                            foreach (string key in form.AllKeys)
                            {
                                AspNetRole role = allRoles.FirstOrDefault(x => x.Name.ToUpper() == key.ToUpper());
                                if (role != null)
                                {
                                    aUser.AspNetRoles.Add(role);
                                }
                            }
                        }
                    aUser.PayrollNo = SITMVCFormHelper.GetStringFromForm(form, "payrollNum");
                    aUser.Email = SITMVCFormHelper.GetStringFromForm(form, "emailAddress");
                    aUser.Name = SITMVCFormHelper.GetStringFromForm(form, "empName");
                    aUser.DOB = SITMVCFormHelper.GetDateTimeFromForm(form, "DOB");
                        context.SaveChanges();

                        //clear all user role
                        //foreach (var role in allRoles)
                        //{
                        //    if (role.Name != "User")
                        //    {
                        //        manager.RemoveFromRole(ExistingID.ToString(), role.Name);
                        //    }
                        //}
                        //foreach (string key in form.AllKeys)
                        //{
                        //    if (allRoles.FirstOrDefault(x => x.Name.ToUpper() == key.ToUpper()) != null)
                        //    {
                        //        manager.AddToRole(ExistingID.ToString(), "User");
                        //    }

                        //}
                    }
                }
                else
                {
                    int ExistingID = SITMVCFormHelper.GetIntegerFromForm(form, "UserID");
                }

                if (IsNopUser)
                {
                    return RedirectToAction("UserSetup", "Home", new { Type = "NOP" });
                }
                else
                {
                    return RedirectToAction("UserSetup", "Home", new { Type = "APP" });
                }

            }
            
        public ActionResult FilterClockings(FormCollection form)
            {
                Guid SiteID = SITMVCFormHelper.GetGuidFromForm(form, "SiteID");
                Guid EmployeeID = SITMVCFormHelper.GetGuidFromForm(form, "EmployeeID");
                DateTime StartDate = SITMVCFormHelper.GetDateTimeFromForm(form, "StartDate");
                DateTime EndDate = SITMVCFormHelper.GetDateTimeFromForm(form, "EndDate");
                bool nShowApproved = SITMVCFormHelper.GetBooleanFromForm(form, "approved");
                bool nShowUnApproved = SITMVCFormHelper.GetBooleanFromForm(form, "unapproved");
                Guid? nSiteID = null;
                if (SiteID != new Guid())
                {
                    nSiteID = SiteID;
                }
                Guid? nEmployeeID = null;
                if (EmployeeID != new Guid())
                {
                    nEmployeeID = EmployeeID;
                }
                DateTime? nStartDate = null;
                if (StartDate > DateTime.MinValue)
                {
                    nStartDate = StartDate;
                }
                DateTime? nEndDate = null;
                if (EndDate > DateTime.MinValue)
                {
                    nEndDate = EndDate;
                }

            return RedirectToAction("Clockings", new { SiteID = nSiteID, EmployeeID = nEmployeeID, StartDate = nStartDate, EndDate = nEndDate, ShowApproved = nShowApproved, ShowUnApproved = nShowUnApproved });
            }

        public ActionResult Clockings(Guid? SiteID, Guid? EmployeeID, DateTime? StartDate, DateTime? EndDate, bool ShowApproved = false, bool ShowUnApproved = true)
        {
            ClockingModel model = new ClockingModel();
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                model.allLocations = context.ClockingLocations.ToList();
                model.allUsers = context.AppUsers.ToList();
                List<Timesheet> timesheets = new List<Timesheet>();
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    timesheets = context.Timesheets.Where(x => x.ClockInDateTime <= EndDate.Value && x.ClockInDateTime >= StartDate.Value).ToList();
                }
                //List<Timesheet> timesheets = context.Timesheets.Where(x => x.ClockInDateTime <= EndDate.Value && x.ClockInDateTime >= StartDate.Value).ToList();
                model.SiteID = SiteID;
                model.EmployeeID = EmployeeID;
                model.StartDate = StartDate;
                model.EndDate = EndDate;
                model.ShowApproved = ShowApproved;
                model.ShowUnApproved = ShowUnApproved;
                //load timesheets.

                if (SiteID.HasValue)
                {
                    timesheets = timesheets.Where(x => x.ClockingLocationID == SiteID.Value).ToList();
                }
                if (EmployeeID.HasValue)
                {
                    timesheets = timesheets.Where(x => x.AppUserID == EmployeeID.Value).ToList();
                }
                if (ShowApproved && ShowUnApproved)
                {
                    //dont filter either
                }
                else
                {
                    if (ShowApproved && !ShowUnApproved)
                    {
                        timesheets = timesheets.Where(x => x.Approved == true).ToList();
                    }
                    else
                    {
                        if (!ShowApproved && ShowUnApproved)
                        {

                            timesheets = timesheets.Where(x => x.Approved == false).ToList();
                        }
                        else
                        {
                            //timesheets.Clear();
                        }
                    }
                }

                model.Timesheets = timesheets;

            }

            return View(model);
        } 


        public ActionResult SaveLocation(FormCollection form)
            {
                Guid lgLocationID = SITMVCFormHelper.GetGuidFromForm(form, "LocationID");
                if (lgLocationID == new Guid())
                {

                    using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                    {
                        ClockingLocation clockLocation = new ClockingLocation();
                        clockLocation.ID = Guid.NewGuid();
                        clockLocation.OrganisationID = SITMVCFormHelper.GetIntegerFromForm(form, "OrganisationID");
                        clockLocation.Latitude = (double)SITMVCFormHelper.GetDecimalFromForm(form, "Latitude");
                        clockLocation.Longitude = (double)SITMVCFormHelper.GetDecimalFromForm(form, "Longitude");
                        clockLocation.Tolerance = SITMVCFormHelper.GetIntegerFromForm(form, "Tolerance");
                        clockLocation.LocationName = form["LocationName"];
                        context.ClockingLocations.Add(clockLocation);
                        context.SaveChanges();
                    }
                }
                else
                {
                    using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                    {
                        ClockingLocation clockLocation = context.ClockingLocations.FirstOrDefault(x => x.ID == lgLocationID);
                        if (clockLocation != null)
                        {
                            clockLocation.OrganisationID = SITMVCFormHelper.GetIntegerFromForm(form, "OrganisationID");
                            clockLocation.Latitude = (double)SITMVCFormHelper.GetDecimalFromForm(form, "Latitude");
                            clockLocation.Longitude = (double)SITMVCFormHelper.GetDecimalFromForm(form, "Longitude");
                            clockLocation.Tolerance = SITMVCFormHelper.GetIntegerFromForm(form, "Tolerance");
                            clockLocation.LocationName = form["LocationName"];
                        }
                        context.SaveChanges();
                    }
                }

                return RedirectToAction("Locations", "Home");

            }
            //public ActionResult Clockings()
            //{
            //    ViewBag.Message = "Your contact page.";

            //    return View();
            //}

            public ActionResult Locations()
            {
                LocationSetupModel model = new LocationSetupModel();
                using (ExpediteTestPortalEntities nopContext = new ExpediteTestPortalEntities())
                {
                    using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                    {
                        model.Locations = context.ClockingLocations.ToList();
                        model.NopOrganisations = nopContext.SIT_Organisation.ToList();
                    }
                }
                return View(model);
            }
        }
    }