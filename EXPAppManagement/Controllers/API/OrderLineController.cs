using EXPAppManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class OrderLineController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<ReturnOrderLineModel> Get(Guid AccessToken, Guid Secret, int OrderId)
        {
            List<ReturnOrderLineModel> model = new List<ReturnOrderLineModel>();
            if (APIHelper.IsAuthenticated(AccessToken, Secret))
            {
                using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
                {
                    List<Nop_OrderProductVariant> orderColl = context.Nop_OrderProductVariant.Include("Nop_ProductVariant").Where(x => x.OrderID == OrderId).ToList();
                    foreach (Nop_OrderProductVariant orderline in orderColl)
                    {
                        ReturnOrderLineModel orderModel = new ReturnOrderLineModel();
                        orderModel.LineID = orderline.OrderProductVariantID;
                        orderModel.Title = orderline.Nop_ProductVariant.Name;
                        orderModel.ProductVariantID = orderline.ProductVariantID;
                        orderModel.Quantity = orderline.Quantity;
                        orderModel.SKU = orderline.Nop_ProductVariant.SKU;
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
    }
}