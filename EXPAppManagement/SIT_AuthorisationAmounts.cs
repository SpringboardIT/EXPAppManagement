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
    
    public partial class SIT_AuthorisationAmounts
    {
        public int AuthorisationAmountId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> AuthoriseeId { get; set; }
        public Nullable<bool> AuthorisationReq { get; set; }
        public Nullable<int> GroupId { get; set; }
        public int eNumType { get; set; }
        public Nullable<int> OverridePriceProfile { get; set; }
    }
}
