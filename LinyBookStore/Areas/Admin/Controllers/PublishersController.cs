using LinyBookStore.Common.Helpers;
using LinyBookStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class PublishersController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/Publisher
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber = (page ?? 1);
            ViewBag.search = search;
            var list = from a in db.Publishers
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list = from a in db.Publishers
                       where a.publisher_name.Contains(search)
                       orderby a.create_at descending
                       select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // Tạo nhà xuất bản
        [HttpPost]
        public JsonResult Create(string publisherName, Publisher publisher)
        {
            string result = "false";
            try
            {
                Publisher checkExist = db.Publishers.SingleOrDefault(m => m.publisher_name == publisherName);
                if (checkExist != null)
                {
                    result = "exist";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                publisher.publisher_name = publisherName;
                publisher.create_by = User.Identity.GetEmail();
                publisher.update_by = User.Identity.GetEmail();
                publisher.create_at = DateTime.Now;
                publisher.update_at = DateTime.Now;
                db.Publishers.Add(publisher);
                db.SaveChanges();
                result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Chỉnh sửa nhà xuất bản
        public JsonResult Edit(int id, string publisherName)
        {
            string result = "error";
            Publisher publisher = db.Publishers.FirstOrDefault(m => m.publisher_id == id);
            var checkExist = db.Publishers.SingleOrDefault(m => m.publisher_name == publisherName);
            try
            {
                if (checkExist != null)
                {
                    result = "exist";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                result = "success";
                publisher.publisher_name = publisherName;
                publisher.update_at = DateTime.Now;
                publisher.update_by = User.Identity.GetEmail();
                db.Entry(publisher).State = EntityState.Modified;
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Xóa nhà xuất bản
        public ActionResult Delete(int id)
        {
            string result = "error";
            Publisher publisher = db.Publishers.FirstOrDefault(m => m.publisher_id == id);
            try
            {
                result = "delete";
                db.Publishers.Remove(publisher);
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