using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using LinyBookStore.Models;
using LinyBookStore.Common.Helpers;
using System.Data.Entity;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/Auth
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber  = (page ?? 1);
            ViewBag.search = search;
            ViewBag.countTrash = db.Accounts.Where(a => a.status == "0").Count();
            var list = from a in db.Accounts
                       where a.status != "0"
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list= from a in db.Accounts
                      where a.email.Contains(search) || a.account_id.ToString().Contains(search) || a.name.Contains(search)
                      orderby a.create_at descending
                      select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        //Hiển thị danh sách bị xóa
        public ActionResult Trash(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber = (page ?? 1);
            ViewBag.search = search;
            var list = from a in db.Accounts
                       where a.status == "0"
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list = from a in db.Accounts
                       where a.email.Contains(search) || a.account_id.ToString().Contains(search) || a.name.Contains(search)
                       orderby a.create_at descending   
                       select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }
        //Chi tiết tài khoản
        public ActionResult Details(int id)
        {
            Account account = db.Accounts.FirstOrDefault(a => a.account_id == id);
            ViewBag.ListAddress = db.Addresses.Where(a => a.account_id == id).ToList();
            if (account == null)
            {
                Notification.setNotification1_5s("Không tồn tại! (ID = " + id + ")", "warning");
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // Thay đổi quyền truy cập tài khoản của người dùng
        public JsonResult ChangeRoles(int accountID, int roleID)
        {
            var account = db.Accounts.FirstOrDefault(a => a.account_id == accountID);
            int role = User.Identity.GetRole(); // Kiểm tra quyền của người dùng có phải là quản trị viên hay không. Nếu không phải, ta không cho phép thay đổi quyền và trả về kết quả false
            bool result = false; 
            try
            {
                if (account != null && role == 0) // Nếu là quản trị viên
                {
                    account.role = roleID;
                    db.Configuration.ValidateOnSaveEnabled = false; // Tắt kiểm tra hợp lệ của Entity Framework và cho phép bạn lưu các thay đổi mà không cần xác nhận rằng chúng hợp lệ
                    db.Entry(account).State = EntityState.Modified; // Đánh dấu tài khoản đã bị thay đổi
                    db.SaveChanges(); // Lưu thay đổi vào CSDL
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet); // Trả đối tượng JSON về phía Client
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //Vô hiệu hóa tài khoản
        public JsonResult Disable(int id)
        {
            string result = "error";
            Account account = db.Accounts.FirstOrDefault(a => a.account_id == id);
            try
            {
                if (User.Identity.GetUserId() != id)
                {
                    result = "success";
                    account.status = "0";
                    db.Configuration.ValidateOnSaveEnabled = false; // Tắt kiểm tra hợp lệ của Entity Framework và cho phép bạn lưu các thay đổi mà không cần xác nhận rằng chúng hợp lệ
                    db.Entry(account).State = EntityState.Modified; // Đánh dấu tài khoản đã bị thay đổi
                    db.SaveChanges();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Kích hoạt tài khoản người dùng
        public JsonResult IsActive(int id)
        {
            string result = "error";
            Account account = db.Accounts.FirstOrDefault(a => a.account_id == id);

            try
            {
                    result = "success";
                    account.status = "1";
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Entry(account).State = EntityState.Modified;
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