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
    
    public partial class SIT_CostCentre
    {
        public int CostCentreID { get; set; }
        public string CostCentreName { get; set; }
        public Nullable<decimal> CostCentreMonthlyBudget { get; set; }
        public Nullable<int> SageCostCentreNumber { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> OrganisationID { get; set; }
        public string CostCentreDescription { get; set; }
    }
}
