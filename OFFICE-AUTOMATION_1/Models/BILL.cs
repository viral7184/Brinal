//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OFFICE_AUTOMATION_1.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BILL
    {
        public int Id { get; set; }
        public int BILL_NO { get; set; }
        public Nullable<int> B_ID { get; set; }
        public Nullable<int> F_ID { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public Nullable<System.DateTime> TO_DATE { get; set; }
        public Nullable<System.DateTime> END_DATE { get; set; }
    
        public virtual bank_master bank_master { get; set; }
        public virtual file file { get; set; }
    }
}
