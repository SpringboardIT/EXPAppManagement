using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EXPAppManagement
{
    public partial class Register : System.Web.UI.Page
    {
        protected void CreateUser_Click(object sender, EventArgs e)
        {
            // Default UserStore constructor uses the default connection string named: DefaultConnection
            var userStore = new UserStore<IdentityUser>();
            var manager = new UserManager<IdentityUser>(userStore);
            var user = new IdentityUser() { UserName = UserName.Text };

            IdentityResult result = manager.Create(user, Password.Text);

            if (result.Succeeded)
            {
                manager.AddToRole(user.Id, "User");
                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                var userIdentity = manager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
                //authenticationManager.SignIn(new AuthenticationProperties() { }, userIdentity);
                AppUser newUser = new AppUser();
                newUser.Name = UserName.Text;
                newUser.ID = Guid.NewGuid();
                newUser.CreatedDate = DateTime.Now;
                if(Admin.Checked)
                {
                    manager.AddToRole(user.Id, "Admin");
                }

                //Add user roles for new users ID
                using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
                {
                    context.AppUsers.Add(newUser);
                    AppUser nUser = context.AppUsers.Where(x => x.Name == UserName.Text).FirstOrDefault();

                    //AspNetUser nUser = context.AspNetUsers.Where(x => x.UserName == UserName.Text).FirstOrDefault();
                    if (newUser != null)
                    {
                        List<AspNetRole> allRoles = context.AspNetRoles.ToList();
                        {
                            AspNetRole role = allRoles.FirstOrDefault(x => x.Name == "Admin");
                            if (role != null)
                            {
                                newUser.AspNetRoles.Add(role);
                                context.SaveChanges();
                            }
                        }
                    }
                    authenticationManager.SignOut();
                    Response.Redirect("~/Login.aspx");
                }
            }
                
            
            else
            {
                StatusMessage.Text = result.Errors.FirstOrDefault();
            }
        }
    }
}