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
    public class LoggingController : ApiController
    {
        // POST: api/Logging
        public int Post(Guid AccessToken, Guid Secret, [FromBody]ClockLogs logs)
        {
            try
            {
                if (APIHelper.IsAuthenticated(AccessToken, Secret))
                {
                    using (var context = new ExpAppClockingsEntities())
                    {
                        int updateCount = 0;
                        AppUser user = context.AppUsers.Where(x => x.AccessToken == AccessToken && x.Secret == Secret).FirstOrDefault();

                        if (logs == null || logs.logList == null || logs.logList.Count == 0)
                        {
                            throw new Exception("Log list is empty");
                        }
                        foreach (ClockLog log in logs.logList)
                        {
                            ClockLogging clockLogging = new ClockLogging()
                            {
                                AppUserID = user.ID.ToString(),
                                ID = log.ID.ToString(),
                                DateCreated = log.dateCreated,
                                DateSent = DateTime.Now,
                                LogDetails = log.logDetails,
                                connectionType = log.connectionType,
                                type = log.type
                            };
                            context.ClockLoggings.Add(clockLogging);
                            updateCount++;
                        }
                        int result = context.SaveChanges();
                        return updateCount;
                    }
                }
                else
                {
                    throw new Exception("User Unauthorised");
                }
            }
            catch (Exception error)
            {
                throw new Exception("Error in posting logs: " + error.Message);
            }
        }

        public class ClockLogs
        {
            public List<ClockLog> logList { get; set; }
        }
        public class ClockLog
        {
            public Guid ID { get; set; }
            public DateTime dateCreated { get; set; }
            public string logDetails { get; set; }
            public DateTime? dateSent { get; set; }
            public string appUser { get; set; }
            public string type { get; set; }
            public string connectionType { get; set; }
        }
    }
}
