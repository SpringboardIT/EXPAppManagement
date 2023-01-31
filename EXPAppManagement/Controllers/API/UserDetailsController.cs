using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class UserDetailsController : ApiController
    {

        // PUT api/<controller>/5
        public bool Put(Guid AccessToken, Guid Secret, [FromBody] UserDetailsInfo userInfo)
        {
            try
            {
                using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                {
                    AppUser appUser = context.AppUsers.FirstOrDefault(x => x.AccessToken == AccessToken && x.Secret == Secret);
                    if (appUser != null)
                    {
                        if (!string.IsNullOrEmpty(userInfo.Name))
                        {
                            appUser.Name = userInfo.Name;
                        }
                        if (!string.IsNullOrEmpty(userInfo.PayrollNo))
                        {
                            appUser.PayrollNo = userInfo.PayrollNo;
                        }
                        if (userInfo.DOB.HasValue)
                        {
                            if (userInfo.DOB.Value.Date < DateTime.Now.Date)
                            {
                                appUser.DOB = userInfo.DOB.Value;
                            }
                        }
                        if (!string.IsNullOrEmpty(userInfo.VerificationEmail))
                        {
                            appUser.VerificationEmail = userInfo.VerificationEmail;
                        }
                        context.SaveChanges();
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
        public class UserDetailsInfo
        {
            public string Name { get; set; }
            public string PayrollNo { get; set; }
            public DateTime? DOB { get; set; }
            public string VerificationEmail { get; set; }
        }
    }
}