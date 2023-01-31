using EXPAppManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class ProductSearchController : ApiController
    {
        // GET api/<controller>
        public ReturnProductsModel Get(Guid AccessToken, Guid Secret, string SearchTerm, int PageSize, int PageIndex)
        {
            using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
            {
                ReturnProductsModel model = new ReturnProductsModel();
                //AND IS ORDERPAD ONLY!!!!!!!!!!!!!!!!!
                model.Products = new List<ProductItem>();
                if (APIHelper.IsAuthenticated(AccessToken, Secret))
                {
                        List<Nop_ProductVariant> prodColl = context.Nop_ProductLoadAllPaged_REST("", null, null, null, null, null, null, null, SearchTerm, null,
                            null, true, null, null, null,null, null, PageIndex, PageSize, false, null).ToList();
                        foreach (Nop_ProductVariant prodvariant in prodColl)
                        {
                            if (model.Products.Where(x => x.SKU == prodvariant.SKU).ToList().Count == 0)
                            {
                                ProductItem prodItem = new ProductItem();
                                prodItem.SKU = prodvariant.SKU;
                                prodItem.Title = prodvariant.Name;
                                prodItem.ProductVariantID = prodvariant.ProductVariantId;
                                model.Products.Add(prodItem);

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
}