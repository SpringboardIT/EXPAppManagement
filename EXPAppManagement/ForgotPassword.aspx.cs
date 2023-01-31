using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI.WebControls;

namespace EXPAppManagement
{
    public partial class ForgotPassword : System.Web.UI.Page
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
                AppUser aUser = context.AppUsers.FirstOrDefault(x => x.Email.ToUpper() == UserName.Text);
                if (aUser != null)
                {
                    if (!string.IsNullOrEmpty(aUser.VerificationEmail))
                    {
                        SendEmail("Forgotten Password Token", "Hello!, Please use the following Token to reset your password: " + aUser.AccessToken.ToString() + aUser.ID.ToString(), "support@springboardit.co.uk", aUser.VerificationEmail, "smtp.office365.com", "", "support@springboardit.co.uk", "Spr1ngB0ard", 587);
                    }
                }
            }

            Response.Redirect("~/ForgotPasswordConfirm.aspx");
        }
        private void SendEmail(string Subject, string Body, string EmailFrom, string EmailTo, string EmailSMTP, string EmailCC, string EmailUser, string EmailPass, int EmailPort)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                Console.WriteLine("Sending Email");
                MailMessage message = new MailMessage();
                message.From = new MailAddress(EmailFrom);
                message.To.Add(EmailTo);
                if (!string.IsNullOrEmpty(EmailCC))
                    message.CC.Add(EmailCC);

                message.Subject = Subject;
                message.IsBodyHtml = true;
                message.Body = Body;
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.TargetName = "STARTTLS/smtp.office365.com";
                smtpClient.Host = EmailSMTP;
                smtpClient.Port = EmailPort;
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new System.Net.NetworkCredential(EmailUser, EmailPass);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                smtpClient.Send(message);
                Console.WriteLine("Sent Email");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Sending Email: " + ex.Message.ToString());
                Console.WriteLine("BODY: " + Body);
            }


        }

    }
}