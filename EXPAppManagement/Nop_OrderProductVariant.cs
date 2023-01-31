//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EXPAppManagement
{
    using System;
    using System.Collections.Generic;
    
    public partial class Nop_OrderProductVariant
    {
        public int OrderProductVariantID { get; set; }
        public int OrderID { get; set; }
        public int ProductVariantID { get; set; }
        public decimal UnitPriceInclTax { get; set; }
        public decimal UnitPriceExclTax { get; set; }
        public decimal PriceInclTax { get; set; }
        public decimal PriceExclTax { get; set; }
        public decimal UnitPriceInclTaxInCustomerCurrency { get; set; }
        public decimal UnitPriceExclTaxInCustomerCurrency { get; set; }
        public decimal PriceInclTaxInCustomerCurrency { get; set; }
        public decimal PriceExclTaxInCustomerCurrency { get; set; }
        public string AttributeDescription { get; set; }
        public string AttributesXML { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountAmountInclTax { get; set; }
        public decimal DiscountAmountExclTax { get; set; }
        public int DownloadCount { get; set; }
        public System.Guid OrderProductVariantGUID { get; set; }
        public bool IsDownloadActivated { get; set; }
        public int LicenseDownloadID { get; set; }
        public Nullable<int> CostCentreID { get; set; }
        public string EmployeeName { get; set; }
    
        public virtual Nop_Order Nop_Order { get; set; }
        public virtual Nop_ProductVariant Nop_ProductVariant { get; set; }
    }
}
