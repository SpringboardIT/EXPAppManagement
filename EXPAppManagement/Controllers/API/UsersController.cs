using EXPAppManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class UsersController : ApiController
    {
        // GET api/<controller>
        //public IEnumerable<APIReturnUser> Get(Guid AccessToken, Guid Secret, string Email = "")
        //{
        //    List<APIReturnUser> model = new List<APIReturnUser>();
        //    if (APIHelper.IsAuthenticated(AccessToken, Secret))
        //    {
        //        using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
        //        {
        //            using (ExpAppClockingsEntities clockingContext = new ExpAppClockingsEntities())
        //            {
        //                List<SIT_Organisation> organisations = context.SIT_Organisation.ToList();
        //                List<Nop_Customer> userColl = context.Nop_Customer.Where(x => x.Deleted == false).ToList();
        //                Guid UserID = new Guid(); //get from user using Email.
        //                DataMappingType dMapType = clockingContext.DataMappingTypes.FirstOrDefault(x => x.eNumMapping == (int)eDataMappingTypes.UserToNopAccount);
        //                List<DataMapping> dataMaps = clockingContext.DataMappings.Where(x => x.PrimaryID == UserID && x.DataMappingType == dMapType.ID).ToList();
        //                foreach (DataMapping dataMap in dataMaps)
        //                {
        //                    Nop_Customer user = userColl.FirstOrDefault(x => x.CustomerID.ToString() == dataMap.SecondaryID);
        //                    //Nop_Customer user = userColl.FirstOrDefault(x => x.CustomerID.ToString() == "519"); //for georges testing prior to user maps.
        //                    if (user != null)
        //                    {
        //                        APIReturnUser userModel = new APIReturnUser();
        //                        userModel.UserID = user.CustomerID;
        //                        userModel.Username = user.Username;
        //                        userModel.Email = user.Email;
        //                        userModel.OrganisationID = user.SITOrganisationID;
        //                        if (user.SITOrganisationID.HasValue)
        //                        {
        //                            SIT_Organisation org = organisations.FirstOrDefault(x => x.OrganisationID == user.SITOrganisationID);
        //                            userModel.OrganisationName = org.OrganisationName;
        //                        }
        //                        model.Add(userModel);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("User Unauthorised");
        //    }
        //    return model;
        //}

        // GET api/<controller>/5
        //public APIReturnUser Get(Guid AccessToken, Guid Secret, int id)
        //{
        //    APIReturnUser model = new APIReturnUser();
        //    if (APIHelper.IsAuthenticated(AccessToken, Secret))
        //    {
        //        using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
        //        {
        //            List<SIT_Organisation> organisations = context.SIT_Organisation.ToList();
        //            Nop_Customer user = context.Nop_Customer.FirstOrDefault(x => x.Deleted == false && x.CustomerID == id);
        //            model.UserID = user.CustomerID;
        //            model.Username = user.Username;
        //            model.Email = user.Email;
        //            model.OrganisationID = user.SITOrganisationID;
        //            if (user.SITOrganisationID.HasValue)
        //            {
        //                SIT_Organisation org = organisations.FirstOrDefault(x => x.OrganisationID == user.SITOrganisationID);
        //                model.OrganisationName = org.OrganisationName;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("User Unauthorised");
        //    }
        //    return model;
        //}
        public ReturnAPIAuthenticateModel Put(Guid AccessToken, Guid Secret, [FromBody] UserInfo userInfo)
        {
            ReturnAPIAuthenticateModel model = new ReturnAPIAuthenticateModel();
            Guid userID = Guid.NewGuid();
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {

                var userStore = new UserStore<IdentityUser>();
                var manager = new UserManager<IdentityUser>(userStore);
              
                if (!userInfo.IsOAuth)
                {
                    var user = new IdentityUser() { UserName = userInfo.Username };
                    IdentityResult result = manager.Create(user, userInfo.Password);
                    manager.AddToRole(user.Id, "User");
                    userID = new Guid(user.Id);
                }

                AppUser appuser = new AppUser();
                appuser.ID = userID;
                appuser.Name = userInfo.Name;
                appuser.Email = userInfo.Email;
                appuser.PayrollNo = userInfo.PayrollNo;
                appuser.DOB = userInfo.DOB;
                appuser.VerificationEmail = userInfo.VerificationEmail;
                appuser.CreatedDate = DateTime.Now;

                context.AppUsers.Add(appuser);
                context.SaveChanges();
            }
            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                AppUser aUser = context.AppUsers.FirstOrDefault(x => x.ID == userID);
                    AspNetRole role = context.AspNetRoles.FirstOrDefault(x => x.Name.ToUpper() == "USER".ToUpper());
                    if (role != null)
                    {
                        aUser.AspNetRoles.Add(role);
                    }
                    context.SaveChanges();


            }
            return model;
        }

        public class UserInfo
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool IsOAuth { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string PayrollNo { get; set; }
            public DateTime DOB { get; set; }
            public string VerificationEmail { get; set; }
        }

    }
}