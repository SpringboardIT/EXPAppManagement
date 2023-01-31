using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace EXPAppManagement
{
    public partial class ForgotPasswordConfirm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (User.Identity.IsAuthenticated)
                {
                    StatusText.Text = string.Format("Hello {0}!!", User.Identity.GetUserName());
                    LoginStatus.Visible = true;
                }
                else
                {
                    LoginForm.Visible = true;
                }
            }
        }

        protected void SignIn(object sender, EventArgs e)
        {
            var userStore = new UserStore<IdentityUser>();
            var userManager = new UserManager<IdentityUser>(userStore);
            var user = userManager.FindByName(UserName.Text);

            using (ExpAppClockingsEntities context = new ExpAppClockingsEntities())
            {

                if ((context.AppUsers.FirstOrDefault(x => x.Email.ToUpper() == UserName.Text).AccessToken.ToString() + context.AppUsers.FirstOrDefault(x => x.Email.ToUpper() == UserName.Text).ID.ToString()).ToString() == VerificationCode.Text)
                {
                    userManager.RemovePassword(user.Id);
                    userManager.AddPassword(user.Id, Password.Text);

                }
            }

            Response.Redirect("~/");
        }

    }
}