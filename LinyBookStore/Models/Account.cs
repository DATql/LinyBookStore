//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LinyBookStore.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Account()
        {
            this.Comments = new HashSet<Comment>();
            this.Addresses = new HashSet<Address>();
            this.Orders = new HashSet<Order>();
            this.RelyComments = new HashSet<RelyComment>();
        }
    
        public int account_id { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string avatar { get; set; }
        public string status { get; set; }
        public System.DateTime create_at { get; set; }
        public string create_by { get; set; }
        public System.DateTime update_at { get; set; }
        public string update_by { get; set; }
        public string requestcode { get; set; }
        public int role { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Address> Addresses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RelyComment> RelyComments { get; set; }
    }
}