using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinyBookStore.Models.DTO
{
    public class ProductDTO
    {
        public int product_id { get; set; }
        public string product_name { get; set;}
        public string genre_name { get; set;}
        public string publisher_name { get; set; }
        public string image { get; set;}
        public double price { get; set; }
        public string quantity { get; set; }
        public string status { get; set; }
        public long view { get; set; }
        public DateTime create_at { get; set; }
        public string create_by { get; set; }
        public DateTime update_at { get; set; }
        public string update_by { get; set; }
        public string description { get; set;}
        public double discount_price { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime discount_start { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime discount_end { get; set; }

        public string discount_name { get; set; }
        public double discount_id { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageUpload { get; set; }
    }
}