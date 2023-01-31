using EXPAppManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class AuthenticateController : ApiController
    {
        // Post api/<controller>
        public ReturnAPIAuthenticateModel Post([FromBody] APIAuthenticateModel AuthItem)
        {
            //validate user/password, get user and return Secret and Token.
            ReturnAPIAuthenticateModel model = new ReturnAPIAuthenticateModel();
            AppUser appuser = null;

            bool UseAccessTokenAndSecret = false;

            if (string.IsNullOrEmpty(AuthItem.Username))
            {
                if (AuthItem.AccessToken.HasValue)
                {
                    UseAccessTokenAndSecret = true;
                }
            }


            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {
                if (UseAccessTokenAndSecret)
                {
                    appuser = context.AppUsers.FirstOrDefault(x => x.AccessToken == AuthItem.AccessToken && x.Secret == AuthItem.Secret);
                    if (appuser != null)
                    {
                        model.Username = appuser.Email;
                        if (model.Roles == null)
                        {
                            model.Roles = new List<string>();
                        }
                        foreach (AspNetRole role in appuser.AspNetRoles)
                        {

                            model.Roles.Add(role.Name);
                        }
                      
                        var userStore = new UserStore<IdentityUser>();
                        var userManager = new UserManager<IdentityUser>(userStore);
                        var user = userManager.FindByName(appuser.Email);
                        if (user != null)
                        {
                            model.Roles.AddRange(userManager.GetRoles(user.Id).ToList());
                        }


                    }
                }
                else
                {
                    var userStore = new UserStore<IdentityUser>();
                    var userManager = new UserManager<IdentityUser>(userStore);
                    IdentityUser user = null;
                    if (AuthItem.IsOAuth)
                    {
                        appuser = context.AppUsers.FirstOrDefault(x => x.Email.ToUpper() == AuthItem.Username.ToUpper());
                        if (appuser != null)
                        {
                            model.Username = appuser.Email;
                            foreach (AspNetRole role in appuser.AspNetRoles)
                            {
                                if (model.Roles == null)
                                {
                                    model.Roles = new List<string>();
                                }
                                model.Roles.Add(role.Name);
                            }
                        }

                    }
                    else
                    {
                        user = userManager.Find(AuthItem.Username, AuthItem.Password);
                        if (user != null)
                        {

                            appuser = context.AppUsers.FirstOrDefault(x => x.ID == new Guid(user.Id));
                            model.Username = appuser.Email;
                            model.Roles = userManager.GetRoles(user.Id).ToList();
                            List<Claim> userClaims = userManager.GetClaims(user.Id).ToList();
                            model.Claims = new List<string>();
                            foreach (Claim cl in userClaims)
                            {
                                model.Claims.Add(cl.Type);
                            }
                        }
                        else
                        {
                            AspNetUser aspUser = context.AspNetUsers.FirstOrDefault(x => x.UserName == AuthItem.Username);
                            if (aspUser == null)
                            {
                                if (AuthItem.CreateIfDoesntExist)
                                {
                                    IdentityUser iUser = new IdentityUser(AuthItem.Username);
                                    IdentityResult res = userManager.Create(iUser, AuthItem.Password);
                                    if (res.Succeeded)
                                    {
                                        AppUser appUser = new AppUser();
                                        appUser.ID = new Guid(iUser.Id);
                                        appUser.Email = AuthItem.Username;
                                        model.Username = AuthItem.Username;
                                        appUser.AccessToken = Guid.NewGuid();
                                        appUser.Secret = Guid.NewGuid();
                                        appUser.CreatedDate = DateTime.Now;
                                        appUser.AppVersion = AuthItem.AppVersion;
                                        context.AppUsers.Add(appUser);
                                        context.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                if (appuser != null)
                {
                    model.NopUsers = new List<APIReturnUser>();
                    List<SIT_Organisation> organisations = null;
                    List<Nop_Customer> userColl = null;
                    using (ExpediteTestPortalEntities nopContext = new ExpediteTestPortalEntities())
                    {
                        organisations = nopContext.SIT_Organisation.Where(x => x.Deleted == false).ToList();
                        userColl = nopContext.Nop_Customer.Where(x => x.Deleted == false).ToList();

                   
                    DataMappingType dMapType = context.DataMappingTypes.FirstOrDefault(x => x.eNumMapping == (int)eDataMappingTypes.UserToNopAccount);
                    List<DataMapping> dataMaps = context.DataMappings.Where(x => x.PrimaryID == appuser.ID && x.DataMappingType == dMapType.ID).ToList();
                    foreach (DataMapping dataMap in dataMaps)
                    {
                        Nop_Customer nopuser = userColl.FirstOrDefault(x => x.CustomerID.ToString() == dataMap.SecondaryID);
                        //Nop_Customer user = userColl.FirstOrDefault(x => x.CustomerID.ToString() == "519"); //for georges testing prior to user maps.
                        if (nopuser != null)
                        {
                            if (nopuser.SITOrganisationID.HasValue)
                            {
                                SIT_Organisation org = organisations.FirstOrDefault(x => x.OrganisationID == nopuser.SITOrganisationID.Value);
                                if (org != null)
                                {
                                    APIReturnUser userModel = new APIReturnUser();
                                    userModel.UserID = nopuser.CustomerID;
                                    userModel.Username = nopuser.Username;
                                    userModel.Email = nopuser.Email;
                                    userModel.OrganisationID = nopuser.SITOrganisationID;

                                    if (org.SIT_Address != null)
                                    {
                                        if (org.SIT_Address.FirstOrDefault() != null)
                                        {
                                            if (!string.IsNullOrEmpty(org.SIT_Address.FirstOrDefault().Address1))
                                            {
                                                userModel.OrganisationName = org.OrganisationName + " " + org.SIT_Address.FirstOrDefault().Address1;
                                            }
                                            else
                                            {
                                                userModel.OrganisationName = org.OrganisationName;
                                            }
                                        }
                                        else
                                        {
                                            userModel.OrganisationName = org.OrganisationName;
                                        }
                                    }
                                    else
                                    {
                                        userModel.OrganisationName = org.OrganisationName;
                                    }
                                  

                                    model.NopUsers.Add(userModel);
                                }
                            }
                        }
                        }
                    }
                }
                else
                {
                    if (AuthItem.CreateIfDoesntExist && AuthItem.IsOAuth)
                    {
                        AppUser appUser = new AppUser();
                        appUser.ID = Guid.NewGuid();
                        appUser.Email = AuthItem.Username;
                        appUser.AccessToken = Guid.NewGuid();
                        appUser.Secret = Guid.NewGuid();
                        appUser.CreatedDate = DateTime.Now;
                        context.AppUsers.Add(appUser);
                        context.SaveChanges();
                    }
                }

                if (appuser != null)
                {
                    appuser.AppVersion = AuthItem.AppVersion;
                    context.SaveChanges();
                }
            }
          
            if (appuser != null)
            {
                if (appuser.AccessToken.HasValue && appuser.Secret.HasValue)
                {
                    model.AccessToken = appuser.AccessToken.Value;
                    model.Secret = appuser.Secret.Value;
                    //model.requestDate = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss");
                }

                if (string.IsNullOrEmpty(appuser.Name))
                {
                    model.HasName = false;
                    if (model.Roles == null)
                    {
                        model.Roles = new List<string>();
                    }
                    if (!model.Roles.Contains("User"))
                    {
                        model.Roles.Add("User");
                    }
                }
                else
                {
                    model.HasName = true;
                    
                }
                if (string.IsNullOrEmpty(appuser.PayrollNo))
                {
                    model.HasPayroll = false;
                    if (model.Roles == null)
                    {
                        model.Roles = new List<string>();
                    }
                    if (!model.Roles.Contains("User"))
                    {
                        model.Roles.Add("User");
                    }
                }
                else
                {
                    model.HasPayroll = true;
                }
                
                model.VerificationEmail = appuser.VerificationEmail;

                if (appuser.DOB.HasValue)
                {
                    model.HasDOB = true;
                }
                else
                {
                    model.HasDOB = false;
                    if (model.Roles == null)
                    {
                        model.Roles = new List<string>();
                    }
                    if (!model.Roles.Contains("User"))
                    {
                        model.Roles.Add("User");
                    }
                }
            }
            model.requestDate = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss");
            return model;
        }

    }
}