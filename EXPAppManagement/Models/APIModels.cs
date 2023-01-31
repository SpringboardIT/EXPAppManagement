using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace EXPAppManagement.Models
{
    public static class APIHelper
    {
        public static bool IsAuthenticated(Guid AccessToken, Guid Secret)
        {
            return true;
        }
    }

    public class APIReturnUser
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int? OrganisationID { get; set; }
        public string OrganisationName { get; set; }

    }
    public class APIAuthenticateModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsOAuth { get; set; }
        public bool CreateIfDoesntExist { get; set; }

        public Guid? AccessToken { get; set; }
        public Guid? Secret { get; set; }

        public string AppVersion { get; set; }
    }
    public class ReturnAPIAuthenticateModel
    {
        public Guid AccessToken { get; set; }
        public Guid Secret { get; set; }

        public List<string> Roles { get; set; }
        public List<string> Claims { get; set; }
        public List<APIReturnUser> NopUsers { get; set; }

        public string requestDate { get; set; }

        public string Username { get; set; }

        public bool HasName { get; set; }
        public bool HasDOB { get; set; }
        public bool HasPayroll { get; set; }
        public string VerificationEmail { get; set; }
    }
    public class GetProductsModel
    {
        public int NopUserID { get; set; }
        public int NopOrganisationID { get; set; }
    }
    public class ReturnOrderModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal OrderTotal { get; set; }
        public string PONumber { get; set; }
        public bool GoneForApproval { get; set; }
    }

    public class ReturnOrderLineModel
    {
        public int LineID { get; set; }
        public int ProductVariantID { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; }

        //public string PictureBase64 { get; set; }
    }


    public class ReturnProductsModel
    {
        public bool IsOrderPadOnly { get; set; }
        public List<ProductItem> Products { get; set; }
    }
    public class ProductItem
    {
        public int ProductVariantID { get; set; }
        public string SKU { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public string Title { get; set; }
        public string PictureBase64 { get; set; }
    }
}