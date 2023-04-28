using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LinyBookStore.Common.Helpers;
using LinyBookStore.Models;
using LinyBookStore.Models.DTO;
using PagedList;
using System.Web.Hosting;
using System.Threading.Tasks;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class AdminProductsController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/AdminProducts
        // Hiển thị toàn bộ danh sách các sản phẩm
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = size ?? 15; // Thiết lập kích thước trang, = null mặc định là 15, != null == size
            var pageNumber = page ?? 1; // Thiết lập số trang, = null mặc định là 1, != null == page
            ViewBag.search = search;
            ViewBag.countTrash = db.Products.Where(a => a.status == "0").Count(); // Đếm tổng sản phẩm có trong thùng rác
            var listProducts = from a in db.Products
                               join b in db.Genres on a.genre_id equals b.genre_id // Kết bảng Thể loại, Nhà xuất bản, Giảm giá
                               join c in db.Publishers on a.publisher_id equals c.publisher_id
                               join d in db.Discounts on a.discount_id equals d.discount_id
                               where a.status == "1"
                               orderby a.create_at descending // Sắp xếp giảm dần theo thời gian tạo
                               select new ProductDTO
                               {
                                   product_id = a.product_id,
                                   product_name = a.product_name,
                                   quantity = a.quantity,
                                   price = a.price,
                                   image = a.image,
                                   view = a.view,
                                   genre_name = b.genre_name,
                                   publisher_name = c.publisher_name,
                                   discount_name = d.discount_name,
                                   discount_price = d.discount_price,
                                   discount_start = (DateTime)d.discount_start,
                                   discount_end = (DateTime)d.discount_end
                               };
            if (!string.IsNullOrEmpty(search))
            {
                listProducts = listProducts.Where(s => s.product_name.Contains(search) || s.product_id.ToString().Contains(search));
            }
            return View(listProducts.ToPagedList(pageNumber, pageSize));
        }

        // Hiển thị toàn bộ danh sách các sản phẩm bị đưa vào thùng rác
        public ActionResult Trash(string search, int? size, int? page)
        {
            var pageSize = size ?? 15; // Thiết lập kích thước trang, = null mặc định là 15, != null == size
            var pageNumber = page ?? 1; // Thiết lập số trang, = null mặc định là 1, != null == page
            ViewBag.search = search;
            ViewBag.countTrash = db.Products.Where(a => a.status == "0").Count(); // Đếm tổng sản phẩm có trong thùng rác
            var listProducts = from a in db.Products
                               join b in db.Genres on a.genre_id equals b.genre_id // Kết bảng Thể loại, Nhà xuất bản, Giảm giá
                               join c in db.Publishers on a.publisher_id equals c.publisher_id
                               join d in db.Discounts on a.discount_id equals d.discount_id
                               where a.status == "0"
                               orderby a.create_at descending // Sắp xếp giảm dần theo thời gian tạo
                               select new ProductDTO
                               {
                                   product_id = a.product_id,
                                   product_name = a.product_name,
                                   quantity = a.quantity,
                                   price = a.price,
                                   image = a.image,
                                   view = a.view,
                                   genre_name = b.genre_name,
                                   publisher_name = c.publisher_name,
                                   discount_name = d.discount_name,
                                   discount_price = d.discount_price,
                                   discount_start = (DateTime)d.discount_start,
                                   discount_end = (DateTime)d.discount_end
                               };
            if (!string.IsNullOrEmpty(search))
            {
                listProducts = listProducts.Where(s => s.product_name.Contains(search) || s.product_id.ToString().Contains(search));
            }
            return View(listProducts.ToPagedList(pageNumber, pageSize));
        }

        // Hiển thị chi tiết sản phẩm
        public ActionResult Details(int? id)
        {
            Product product = db.Products.FirstOrDefault(m => m.product_id == id);
            if (product == null)
            {
                Notification.setNotification1_5s("Không tồn tại! (ID = " + id + ")", "warning");
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Tạo mới sách
        public ActionResult Create()
        {
            ViewBag.ListGenre = new SelectList(db.Genres, "genre_id", "genre_name", 0);
            ViewBag.ListPublisher = new SelectList(db.Publishers, "publisher_id", "publisher_name", 0);
            ViewBag.ListDiscount = new SelectList(db.Discounts.OrderBy(m => m.discount_price), "discount_id", "discount_name", 0);
            return View();
        }

        // Code xử lý tạo mới sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Product product, Product_Img productImage)
        {
            ViewBag.ListGenre = new SelectList(db.Genres, "genre_id", "genre_name", 0);
            ViewBag.ListPublisher = new SelectList(db.Publishers, "publisher_id", "publisher_name", 0);
            ViewBag.ListDiscount = new SelectList(db.Discounts.OrderBy(m => m.discount_price), "discount_id", "discount_name", 0);
            try
            {
                if (product.Product_Img != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(product.ImageUpload.FileName);
                    var extension = Path.GetExtension(product.ImageUpload.FileName);
                    fileName = fileName + DateTime.Now.ToString("HH-mm-dd-MM-yyyy") + extension;
                    product.image = "/Content/Images/product/" + fileName;
                    product.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/Images/product/"), fileName));
                }
                else
                {
                    Notification.setNotification3s("Vui lòng thêm Ảnh!", "error");
                    return View(product);
                }
                product.status = "1";
                product.view = 0;
                product.buyturn = 0;
                product.type = product.type;
                product.product_information = product.product_information;
                product.description = product.description;
                product.create_at = DateTime.Now;
                product.create_by = User.Identity.GetUserId().ToString();
                product.update_at = DateTime.Now;
                product.update_by = User.Identity.GetUserId().ToString();
                db.Products.Add(product);
                db.SaveChanges();
                foreach (HttpPostedFileBase image_multi in product.ImageUploadMulti)
                {
                    if (image_multi != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(image_multi.FileName);
                        var extension = Path.GetExtension(image_multi.FileName);
                        fileName = fileName + DateTime.Now.ToString("HH-mm-dd-MM-yyyy") + extension;
                        productImage.image = "/Content/Images/product/" + fileName;
                        image_multi.SaveAs(Path.Combine(Server.MapPath("~/Content/Images/product/"), fileName));
                        productImage.product_id = product.product_id;
                        productImage.discount_id = product.discount_id;
                        productImage.genre_id = product.genre_id;
                        db.Product_Img.Add(productImage);
                        db.SaveChanges();
                    }
                }
                Notification.setNotification1_5s("Thêm mới sách thành công!", "success");
                return RedirectToAction("Index");
            }
            catch
            {
                Notification.setNotification1_5s("Lỗi", "error");
                return View(product);
            }
        }

        // Chỉnh sửa sách
        public ActionResult Edit(int? id)
        {
            ViewBag.ListGenre = new SelectList(db.Genres, "genre_id", "genre_name", 0);
            ViewBag.ListPublisher = new SelectList(db.Publishers, "publisher_id", "publisher_name", 0);
            ViewBag.ListDiscount = new SelectList(db.Discounts.OrderBy(m => m.discount_price), "discount_id", "discount_name", 0);
            var product = db.Products.FirstOrDefault(x => x.product_id == id);
            if (product == null || id == null)
            {
                Notification.setNotification1_5s("Không tồn tại! (ID = " + id + ")", "warning");
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Product model, Product_Img productImage)
        {
            ViewBag.ListGenre = new SelectList(db.Genres, "genre_id", "genre_name", 0);
            ViewBag.ListPublisher = new SelectList(db.Publishers, "publisher_id", "publisher_name", 0);
            ViewBag.ListDiscount = new SelectList(db.Discounts.OrderBy(m => m.discount_price), "discount_id", "discount_name", 0);
            var product = db.Products.SingleOrDefault(x => x.product_id == model.product_id);
            try
            {
                if (model.ImageUpload != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(model.ImageUpload.FileName);
                    var extension = Path.GetExtension(model.ImageUpload.FileName);
                    fileName = fileName + extension;
                    product.image = "/Content/Images/product/" + fileName;
                    model.ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Content/Images/product/"), fileName));
                }
                product.product_name = model.product_name;
                product.quantity = model.quantity;
                product.description = model.description;
                product.product_information = model.product_information;
                product.price = model.price;
                product.publisher_id = model.publisher_id;
                product.type = model.type;
                product.update_at = DateTime.Now;
                product.update_by = User.Identity.GetName();
                db.Entry(product).State = EntityState.Modified; //  Cập nhật dữ liệu trong cơ sở dữ liệu trong Entity Framework.
                db.SaveChanges(); // Để khi SaveChanges() được gọi, Entity Framework sẽ tự động tìm và cập nhật bản ghi tương ứng trong cơ sở dữ liệu.
                Notification.setNotification1_5s("Đã cập nhật lại thông tin!", "success");
                return RedirectToAction("Index");
            }
            catch
            {
                Notification.setNotification1_5s("Lỗi", "error");
                return View(model);
            }
        }

        // Vô hiệu hóa sách
        public JsonResult Disable(int id)
        {
            string result = "error";
            Product product = db.Products.FirstOrDefault(m => m.product_id == id);
            try
            {
                result = "disabled";
                product.status = "0";
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Hủy bỏ hành động
        public ActionResult Undo(int id)
        {
            string result = "error";
            Product product = db.Products.FirstOrDefault(m => m.product_id == id);
            try
            {
                result = "activate";
                product.status = "1";
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Xóa sách
        public JsonResult Delete(int id)
        {
            string result = "error";
            Product product = db.Products.FirstOrDefault(m => m.product_id == id);
            try
            {
                List<Product_Img> listImage = db.Product_Img.Where(m => m.product_id == id).ToList();
                foreach (var item in listImage)
                {
                    db.Product_Img.Remove(item);
                    db.SaveChanges();
                }
                result = "delete";
                db.Products.Remove(product);
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}