using EXPAppManagement.Models;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EXPAppManagement.Controllers.API
{
    public class ProductsController : ApiController
    {


        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            return (byte[])(new ImageConverter()).ConvertTo(imageIn, typeof(byte[]));
        }

        // GET api/<controller>
        public ReturnProductsModel Get(Guid AccessToken, Guid Secret, int NopUserID)
        {
            using (ExpediteTestPortalEntities context = new ExpediteTestPortalEntities())
            {
                ReturnProductsModel model = new ReturnProductsModel();
                //AND IS ORDERPAD ONLY!!!!!!!!!!!!!!!!!
                model.Products = new List<ProductItem>();
                if (APIHelper.IsAuthenticated(AccessToken, Secret))
                {
                    bool isOrderPadOnly = true;
                    Nop_CustomerAttribute OrderPadOnly = context.Nop_CustomerAttribute.FirstOrDefault(x => x.CustomerId == NopUserID && x.Key == "OrderPadOnly");
                    if (OrderPadOnly != null)
                    {
                        if (OrderPadOnly.Value.ToUpper() == "FALSE")
                        {
                            isOrderPadOnly = false;
                        }
                    }
                    model.IsOrderPadOnly = isOrderPadOnly;
                        List<SIT_OrderPad> ordPadColl = context.SIT_OrderPadLoadByCustomerID(NopUserID, 3).ToList();
                        foreach (SIT_OrderPad ordPad in ordPadColl)
                        {
                            List<Nop_ProductVariant> prodColl = context.SIT_ProductVariantLoadByOrderPadID(ordPad.OrderPadID, 4).ToList();
                            foreach (Nop_ProductVariant prodvariant in prodColl)
                            {
                                if (model.Products.Where(x => x.SKU == prodvariant.SKU).ToList().Count == 0)
                                {
                                    ProductItem prodItem = new ProductItem();
                                    prodItem.SKU = prodvariant.SKU;
                                    prodItem.Title = prodvariant.Name;
                                prodItem.ProductVariantID = prodvariant.ProductVariantId;
                                    prodItem.MinimumOrderQuantity = prodvariant.OrderMinimumQuantity;
                                    Nop_Picture picture = context.Nop_Picture.FirstOrDefault(x => x.PictureID == prodvariant.PictureID);
                                if (picture != null)
                                {
                                    Image x = (Bitmap)((new ImageConverter()).ConvertFrom(picture.PictureBinary));

                                    int NewWidth = 100;
                                    int OldWidth = x.Width;

                                    decimal Difference = OldWidth / NewWidth; //150 / 100 = 1.5

                                    decimal NewHeight = x.Height / Difference;


                                    x = resizeImage(x, new Size(NewWidth, (int)NewHeight));
                                    try
                                    {
                                        string base64ImageRepresentation = Convert.ToBase64String(ImageToByteArray(x));
                                        prodItem.PictureBase64 = base64ImageRepresentation;
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                   
                                }
                                else
                                {
                                    prodItem.PictureBase64 = "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";
                                }
                                    model.Products.Add(prodItem);

                                }
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