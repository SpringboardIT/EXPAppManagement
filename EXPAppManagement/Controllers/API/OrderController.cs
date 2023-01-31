using EXPAppManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class OrderController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<ReturnOrderModel> Get(Guid AccessToken, Guid Secret, int NopUserID)
        {
            List<ReturnOrderModel> model = new List<ReturnOrderModel>();
            if (APIHelper.IsAuthenticated(AccessToken, Secret))
            {
                using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
                {
                    List<Nop_Order> orderColl = context.Nop_Order.Where(x => string.IsNullOrEmpty(x.TrackingNumber) && x.CustomerID == NopUserID).OrderByDescending(x => x.CreatedOn).Take(10).ToList();
                    foreach (Nop_Order order in orderColl)
                    {
                        ReturnOrderModel orderModel = new ReturnOrderModel();
                        orderModel.OrderID = order.OrderID;
                        orderModel.OrderDate = order.CreatedOn;
                        orderModel.OrderTotal = order.OrderTotal;
                        orderModel.PONumber = order.PurchaseOrderNumber;
                        orderModel.GoneForApproval = order.Deleted;
                        model.Add(orderModel);
                    }
                }
            }
            else
            {
                throw new Exception("User Unauthorised");
            }
            return model;
        }

        public class SendInOrderHeader
        {
            public Guid ID { get; set; }
            public DateTime OrderDate { get; set; }
            public int NopUserID { get; set; }
            public List<SendInOrderLine> Lines { get; set; }
        }
        public class SendInOrderLine
        {
            public string SKU { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public Guid HeaderID { get; set; }
            public int ProductVariantID { get; set; }
        }

        public class SendOutOrder
        {
            public Guid AppOrderID { get; set; }
            public int NopOrderID { get; set; }
            public int Status { get; set; }
        }


        public SendOutOrder Put(Guid AccessToken, Guid Secret, [FromBody] SendInOrderHeader order)
        {
            SendOutOrder Model = new SendOutOrder();
            Model.AppOrderID = order.ID;
            Model.Status = 999;
            try
            {

            
            SortedList<int, decimal> ProdVarsAndUnitPrices = new SortedList<int, decimal>();



            using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
            {
                Nop_Order ord = context.Nop_Order.FirstOrDefault(x => x.OrderGUID == order.ID);
                if (ord != null)
                {
                    Model.AppOrderID = order.ID;
                    Model.NopOrderID = ord.OrderID;
                    Model.Status = 200;
                }
                else
                {

                    Nop_Customer cust = context.Nop_Customer.FirstOrDefault(x => x.CustomerID == order.NopUserID);
                    if (cust != null)
                    {

                        SIT_Organisation org = context.SIT_Organisation.FirstOrDefault(x => x.OrganisationID == cust.SITOrganisationID);

                        if (org != null)
                        {
                            Guid orderGUID = Guid.NewGuid();
                            string PONumber = string.Empty;
                            int AuthoriserID = 0;
                            string orgInitials = org.OrganisationName.Substring(0, Math.Min(org.OrganisationName.Length, 3));
                            string shorttime = DateTime.Now.ToString("ddMMyy/Hmmss");
                            // shorttime = shorttime.Replace(":", "");
                            shorttime = shorttime.Replace(" ", "");
                            PONumber = orgInitials.ToUpper() + "/" + shorttime;

                            SIT_CostCentre cc = context.SIT_CostCentreLoadByCustomerID(cust.CustomerID, 7).FirstOrDefault();
                            SIT_Address add = context.SIT_AddressesForUser(cust.SITOrganisationID, 2, cust.CustomerID).FirstOrDefault();

                            decimal OrderTotalExclTax = 0;
                            //if (!org.IsZeroPricing)
                            //{
                            foreach (SendInOrderLine line in order.Lines)
                            {
                                ObjectParameter Price = new ObjectParameter("Price", typeof(global::System.Decimal));
                                var priceresult = context.SIT_GetSalePrice(Price, org.OrganisationID, org.PriceProfile, 0, DateTime.Now, line.ProductVariantID, 0, 0, true);
                                if (Price.Value != null)
                                {
                                    if (!ProdVarsAndUnitPrices.ContainsKey(line.ProductVariantID))
                                    {
                                        ProdVarsAndUnitPrices.Add(line.ProductVariantID, (decimal)Price.Value);
                                    }
                                    OrderTotalExclTax += ((decimal)Price.Value * line.Quantity);
                                }
                                else
                                {
                                    if (!ProdVarsAndUnitPrices.ContainsKey(line.ProductVariantID))
                                    {
                                        ProdVarsAndUnitPrices.Add(line.ProductVariantID, 0);
                                    }
                                }
                            }
                            //}


                            decimal OrderTotalInclTax = OrderTotalExclTax * (decimal)1.2;//get tax from DB

                            bool RequiresAuthorisation = false;

                            Nop_CustomerAttribute customerAttr = context.Nop_CustomerAttribute.FirstOrDefault(x => x.CustomerId == order.NopUserID && x.Key == "AuthorizationGroup");
                            if (customerAttr != null)
                            {
                                int AuthorisationGroupID = 0;
                                int.TryParse(customerAttr.Value, out AuthorisationGroupID);
                                if (AuthorisationGroupID > 0)
                                {
                                    SIT_AuthorisationGroups authGroup = context.SIT_AuthorisationGroups.FirstOrDefault(x => x.AuthorisationGroupId == AuthorisationGroupID);
                                    if (authGroup != null)
                                    {
                                        List<SIT_AuthorisationAmounts> amounts = context.SIT_AuthorisationAmounts.Where(x => x.GroupId == authGroup.AuthorisationGroupId).ToList();
                                        foreach (SIT_AuthorisationAmounts authamount in amounts)
                                        {
                                            if (authamount.eNumType == 0)
                                            {
                                                if (OrderTotalExclTax > authamount.Amount)
                                                {
                                                    RequiresAuthorisation = true;
                                                    if (authamount.AuthoriseeId.HasValue)
                                                    {
                                                        AuthoriserID = authamount.AuthoriseeId.Value;
                                                    }
                                                    else
                                                    {
                                                        if (authGroup.AuthoriseeId.HasValue)
                                                        {
                                                            AuthoriserID = authGroup.AuthoriseeId.Value;
                                                        }
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                DateTime eightWeeksAgo = DateTime.Now.Date.AddDays(-56);
                                                decimal SpendInLastEightWeeks = 0;
                                               List <Nop_Order> recentOrders = context.Nop_Order.Where(x => x.CustomerID == order.NopUserID && x.Deleted == false && x.CreatedOn > eightWeeksAgo).ToList();
                                                if (recentOrders.Count > 0)
                                                {
                                                    SpendInLastEightWeeks = recentOrders.Sum(x => x.OrderTotal);
                                                }
                                                int PriceProfile = 0;
                                                if (authamount.OverridePriceProfile.HasValue)
                                                {
                                                    PriceProfile = authamount.OverridePriceProfile.Value;
                                                }
                                                else
                                                {
                                                    PriceProfile = org.PriceProfile.Value;
                                                }
                                                decimal OrderTotal = 0;
                                                bool ForceAuth = false;
                                                foreach (SendInOrderLine line in order.Lines)
                                                {
                                                    ObjectParameter Price = new ObjectParameter("Price", typeof(global::System.Decimal));
                                                    //calc orderTotal from new One.
                                                    if (Price.Value == null)
                                                    {

                                                        ForceAuth = true;
                                                    }
                                                    else
                                                    {
                                                        var priceresult = context.SIT_GetSalePrice(Price, org.OrganisationID, PriceProfile, 0, DateTime.Now, line.ProductVariantID, 0, 0, false);
                                                        OrderTotal += ((decimal)Price.Value * line.Quantity);
                                                    }

                                                }
                                                if (((SpendInLastEightWeeks + OrderTotal) > (authamount.Amount * 2)) || ForceAuth)
                                                {
                                                    RequiresAuthorisation = true;
                                                    if (authamount.AuthoriseeId.HasValue)
                                                    {
                                                        AuthoriserID = authamount.AuthoriseeId.Value;
                                                    }
                                                    else
                                                    {
                                                        if (authGroup.AuthoriseeId.HasValue)
                                                        {
                                                            AuthoriserID = authGroup.AuthoriseeId.Value;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }



                            ObjectParameter ordID = new ObjectParameter("OrderID", typeof(global::System.Int32));
                            var result = context.Nop_OrderInsert(ordID, orderGUID, order.NopUserID, 7, 2, string.Empty, OrderTotalInclTax, OrderTotalExclTax, 0, 0, 0, 0,
                                (OrderTotalInclTax - OrderTotalExclTax), OrderTotalInclTax, 0, OrderTotalInclTax, OrderTotalExclTax, 0, 0, 0, 0, (OrderTotalInclTax - OrderTotalExclTax)
                                , OrderTotalInclTax, 0,
                                string.Empty, string.Empty, ".", 0, 0, 20, false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                string.Empty, string.Empty, 18, "Purchase Order", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty
                                , PONumber, 30, DateTime.Now, add.FirstName, add.LastName, string.Empty, cust.Email, string.Empty, org.OrganisationName, add.Address1, add.Address2, add.City, string.Empty, 0, add.ZipPostalCode, string.Empty,
                                0, 10, add.FirstName, add.LastName, string.Empty, cust.Email, string.Empty, org.OrganisationName, add.Address1, add.Address2, add.City, string.Empty, 0, add.ZipPostalCode, string.Empty,
                                0, string.Empty, 0, null, null, string.Empty, RequiresAuthorisation, DateTime.Now, string.Empty);

                            if (ordID != null)
                            {
                                if (ordID.Value != null)
                                {
                                    Model.NopOrderID = (int)ordID.Value;
                                    Model.Status = 500;
                                }
                            }

                            string basketitems = string.Empty;
                            basketitems = "<table border='1' style='font-family:arial'><tr><th width='100px'>Product Code</th><th width='300px'>Name</th><th width='75px'>Price</th><th width='75px'>Quantity</th></tr>";

                            if (Model.NopOrderID > 0)
                            {
                                foreach (SendInOrderLine line in order.Lines)
                                {
                                    if (line.Quantity > 0)
                                    {
                                        decimal linePriceExlTax = 0;
                                        //if (!org.IsZeroPricing)
                                        //{
                                        if (ProdVarsAndUnitPrices.ContainsKey(line.ProductVariantID))
                                        {
                                            linePriceExlTax = ProdVarsAndUnitPrices[line.ProductVariantID];
                                        }
                                        //}

                                        decimal linePriceInclTax = linePriceExlTax * (decimal)1.2; //add tax


                                        ObjectParameter ordpvID = new ObjectParameter("OrderProductVariantID", typeof(global::System.Int32));
                                        if (cc != null)
                                        {
                                            context.Nop_OrderProductVariantInsert(ordpvID, Guid.NewGuid(), Model.NopOrderID, line.ProductVariantID, linePriceInclTax, linePriceExlTax, (linePriceInclTax * line.Quantity),
                                            (linePriceExlTax * line.Quantity), linePriceInclTax,
                                            linePriceExlTax, (linePriceInclTax * line.Quantity),
                                            (linePriceExlTax * line.Quantity), string.Empty, string.Empty, line.Quantity, 0, 0, 0, false, 0, cc.CostCentreID, string.Empty);
                                        }
                                        else
                                        {
                                            context.Nop_OrderProductVariantInsert(ordpvID, Guid.NewGuid(), Model.NopOrderID, line.ProductVariantID, linePriceInclTax, linePriceExlTax, (linePriceInclTax * line.Quantity),
                                            (linePriceExlTax * line.Quantity), linePriceInclTax,
                                            linePriceExlTax, (linePriceInclTax * line.Quantity),
                                            (linePriceExlTax * line.Quantity), string.Empty, string.Empty, line.Quantity, 0, 0, 0, false, 0, 0, string.Empty);
                                        }
                                        Nop_ProductVariant pv = context.Nop_ProductVariant.FirstOrDefault(x => x.ProductVariantId == line.ProductVariantID);
                                        if (pv != null)
                                        {
                                            basketitems = basketitems + "<tr valign='middle'><td align='center'>" + pv.SKU + "</td><td align='center'>" + pv.Name + "</td><td align='center'>£" + linePriceInclTax + "</td><td align='center'>" + line.Quantity + "</td></tr>";
                                        }


                                        Model.Status = 300;
                                    }
                                }

                                if (!RequiresAuthorisation)
                                {

                                    ExpediteNop.SpicersLoadSoapClient client = new ExpediteNop.SpicersLoadSoapClient();
                                    client.SendOrderToSOPA((int)ordID.Value);
                                }
                                else
                                {
                                    basketitems = basketitems + "</table></p><br/>Sub Total: £" + OrderTotalExclTax.ToString();
                                    string body = string.Empty;
                                    body = "<html><body style='font-family:arial'><center><span style='font-weight:bold'>Order Requires Authorisation</span><br/>";
                                    Nop_CustomerAttribute firstname = context.Nop_CustomerAttribute.FirstOrDefault(x => x.CustomerId == order.NopUserID && x.Key == "FirstName");
                                    Nop_CustomerAttribute lastname = context.Nop_CustomerAttribute.FirstOrDefault(x => x.CustomerId == order.NopUserID && x.Key == "LastName");
                                    if (firstname != null && lastname != null)
                                    {
                                        body = body + "Placed by: " + firstname.Value + " " + lastname.Value + "<br/>";
                                    }
                                    else
                                    {
                                        body = body + "Placed by: " + cust.Username + "<br/>";
                                    }
                                    int ccID = 0;
                                    if (cc != null)
                                    {
                                        ccID = cc.CostCentreID;
                                    }
                                    body = body + "<br/><br/>To authorise this order please follow the following link: <a href='http://theprocurementportal.org.uk/ApproveOrder.aspx?CCID=" + ccID + "&PON=" + PONumber + "&AddID=" + add.AddressId + "&ORDERGUID=SITORDER_ID'>Authorise Order</a>";
                                    body = body + "<br/><br/>To reject this order please follow the following link: <a href='http://theprocurementportal.org.uk/RejectOrder.aspx?CCID=" + ccID + "&PON=" + PONumber + "&AddID=" + add.AddressId + "&ORDERGUID=SITORDER_ID'>Reject Order</a>";
                                    body = body + "<br/><br/>To amend this order please follow the following link: <a href='http://theprocurementportal.org.uk/AmendOrder.aspx?CCID=" + ccID + "&PON=" + PONumber + "&AddID=" + add.AddressId + "&ORDERGUID=SITORDER_ID'>Amend Order</a>";
                                    body = body + "<br/><br/>Any queries with the authorisation of order please email: Sales@expedite.uk.com";
                                    body = body + "<br/><br/><span style='font-weight:bold'>Order Summary <br/>Purchase Order Number: " + PONumber + "<br/>";
                                    if (cc != null)
                                    {
                                        body = body + "Cost Centre: " + cc.CostCentreName + "<br/><br/>Address:<br/></span>";
                                    }
                                    else
                                    {
                                        body = body + "Address:<br/></span>";
                                    }

                                    body = body + add.Address1 + "<br/>" + add.Address2 + "<br/>";
                                    body = body + add.City + "<br/>" + add.ZipPostalCode + "<br/><br/>";
                                    body = body + basketitems + "</p></center></body></html>";
                                    //Send Email!


                                    string OrderGuidStr = orderGUID.ToString();
                                    Nop_Customer authCustomer = context.Nop_Customer.FirstOrDefault(x => x.CustomerID == AuthoriserID);
                                    if (authCustomer != null)
                                    {
                                        if (authCustomer.Email.Contains(";"))
                                        {
                                            if (authCustomer.Email.Contains(" "))
                                            {
                                                authCustomer.Email = cust.Email.Replace(" ", "");
                                            }
                                            string[] authorisers = authCustomer.Email.Split(new Char[] { ';' });
                                            foreach (string auth in authorisers)
                                            {
                                                if (!string.IsNullOrEmpty(auth))
                                                {
                                                    SendEmail("Expedite Order requires Authorising", body.Replace("SITORDER_ID", OrderGuidStr), "support@springboardit.co.uk", auth, "smtp.office365.com", "", "support@springboardit.co.uk", "Spr1ngB0ard", 587);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (authCustomer.Email.Contains(" "))
                                            {
                                                authCustomer.Email = authCustomer.Email.Replace(" ", "");
                                            }
                                            SendEmail("Expedite Order requires Authorising", body.Replace("SITORDER_ID", OrderGuidStr), "support@springboardit.co.uk", authCustomer.Email, "smtp.office365.com", "", "support@springboardit.co.uk", "Spr1ngB0ard", 587);
                                        }
                                    }
                                    SendEmail("Expedite Order requires Authorising", "<html><body><center>Your order has been placed for authorising, your authorisor will now review this request<br/></br>Any further queries please contact Sales@expedite.uk.com</center></body></html>", "support@springboardit.co.uk", cust.Email, "smtp.office365.com", "", "support@springboardit.co.uk", "Spr1ngB0ard", 587);

                                }
                                Model.Status = 200;

                            }

                        }
                        else
                        {
                            Model.Status = 111;
                        }
                    }
                }
            }
            }
            catch (Exception ex)
            {
                //we need to add this to the model. 
            }
            return Model;
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


                //foreach (string Attachment in Attachments)
                //{
                //    message.Attachments.Add(new Attachment(Attachment));
                //}

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