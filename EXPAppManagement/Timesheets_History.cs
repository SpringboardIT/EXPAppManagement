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
    
    public partial class Timesheets_History
    {
        public System.Guid UniqueID { get; set; }
        public System.DateTime ClockInDateTime { get; set; }
        public Nullable<System.DateTime> ClockOutDateTime { get; set; }
        public Nullable<System.Guid> AppUserID { get; set; }
        public Nullable<System.Guid> ClockingLocationID { get; set; }
        public string Comments { get; set; }
        public bool Approved { get; set; }
        public Nullable<System.Guid> ApprovedBy { get; set; }
        public Nullable<System.Guid> ClockInTransaction { get; set; }
        public Nullable<System.Guid> ClockOutTransaction { get; set; }
        public Nullable<int> Duration { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public string UserEmailAddress { get; set; }
        public Nullable<double> ClockInLat { get; set; }
        public Nullable<double> ClockInLong { get; set; }
        public Nullable<double> ClockOutLat { get; set; }
        public Nullable<double> ClockOutLong { get; set; }
        public string OutOfRangeClockInJustification { get; set; }
        public string OutOfRangeClockOutJustification { get; set; }
    }
}
