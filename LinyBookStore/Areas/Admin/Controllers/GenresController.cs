using LinyBookStore.Common.Helpers;
using LinyBookStore.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LinyBookStore.Areas.Admin.Controllers
{ 
    public class GenresController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/Genre
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber = (page ?? 1);
            ViewBag.search = search;
            var list = from a in db.Genres
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list = from a in db.Genres
                       where a.genre_name.Contains(search)
                       orderby a.create_at descending
                       select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        // Tạo thể loại sách
        [HttpPost]
        public JsonResult Create(string genreName, Genre genre)
        {
            string result = "false";
            try
            {
                Genre checkExist = db.Genres.SingleOrDefault(m => m.genre_name == genreName);
                if (checkExist != null)
                {
                    result = "exist";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                genre.genre_name = genreName;
                genre.create_by = User.Identity.GetEmail();
                genre.update_by = User.Identity.GetEmail();
                genre.create_at = DateTime.Now;
                genre.update_at = DateTime.Now;
                db.Genres.Add(genre);
                db.SaveChanges();
                result = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Chỉnh sửa thể loại sách
        public JsonResult Edit(int id, string genreName)
        {
            string result = "error";
            Genre genre = db.Genres.FirstOrDefault(m => m.genre_id == id);
            var checkExist = db.Genres.SingleOrDefault(m => m.genre_name == genreName);
            try
            {
                if (checkExist != null)
                {
                    result = "exist";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                result = "success";
                genre.genre_name = genreName;
                genre.update_at = DateTime.Now;
                genre.update_by = User.Identity.GetEmail();
                db.Entry(genre).State = EntityState.Modified;
                db.SaveChanges();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Xóa thể loại sách
        public ActionResult Delete(int id)
        {
            string result = "error";
            Genre genre = db.Genres.FirstOrDefault(m => m.genre_id == id);
            try
            {
                result = "delete";
                db.Genres.Remove(genre);
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