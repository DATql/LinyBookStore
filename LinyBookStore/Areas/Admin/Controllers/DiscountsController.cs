using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using LinyBookStore.Models;
using System.Globalization;
using LinyBookStore.Common.Helpers;
using System.Data.Entity;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class DiscountsController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/Discount
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber = (page ?? 1);
            ViewBag.search = search;
            var list = from a in db.Discounts
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list = from a in db.Discounts
                       where a.discount_name.Contains(search) || a.discount_price.ToString().Contains(search)
                       orderby a.create_at descending
                       select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // Tạo mã khuyến mãi
        [HttpPost]
        public JsonResult Create(DateTime discountStart, DateTime discountEnd, double discountPrice, string discountCode, Discount discount, int quantity)
        {
            string result = "false";
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            try
            {
                discount.discount_name = "Giảm " +
                        discountPrice.ToString("#,0₫", cul.NumberFormat) + " Từ " +
                        discountStart.ToString("dd-MM-yyyy") + " => " +
                        discountEnd.ToString("dd-MM-yyyy");
                discount.discount_price = discountPrice;
                discount.quantity = quantity;
                discount.discount_start = discountStart;
                discount.discount_end = discountEnd;
                discount.discount_code = discountCode;
                discount.create_by = User.Identity.GetEmail();
                discount.update_by = User.Identity.GetEmail();
                discount.create_at = DateTime.Now;
                discount.update_at = DateTime.Now;
                db.Discounts.Add(discount);
                db.SaveChanges();
                result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Chỉnh sửa mã giảm giá
        public JsonResult Edit(int id, DateTime discountStart, DateTime discountEnd, double discountPrice, string discountCode, int quantity)
        {
            string result = "error";
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            Discount discount = db.Discounts.FirstOrDefault(m => m.discount_id == id);
            try
            {
                discount.discount_name = "Giảm " +
                discountPrice.ToString("#,0₫", cul.NumberFormat) + " Từ " +
                discountStart.ToString("dd-MM-yyyy") + " => " +
                discountEnd.ToString("dd-MM-yyyy");
                discount.discount_price = discountPrice;
                discount.discount_start = discountStart;
                discount.discount_end = discountEnd;
                discount.quantity = quantity;
                discount.discount_code = discountCode;
                discount.update_at = DateTime.Now;
                discount.update_by = User.Identity.GetEmail();
                db.Entry(discount).State = EntityState.Modified;
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Xóa mã giảm giá
        public ActionResult Delete(int id)
        {
            string result = "error";
            Discount discount = db.Discounts.FirstOrDefault(m => m.discount_id == id);
            try
            {
                result = "delete";
                db.Discounts.Remove(discount);
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Giải phóng tài nguyên và thu hồi bộ nhớ được sử dụng bởi đối tượng context của cơ sở dữ liệu. Phương thức được gọi tự động khi đối tượng controller được giải phóng khỏi bộ nhớ.
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}