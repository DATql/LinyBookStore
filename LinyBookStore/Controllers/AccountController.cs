using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Web.Helpers;
using System.Web.Hosting;
using PagedList;
using Newtonsoft.Json;
using LinyBookStore.Common;
using LinyBookStore.Common.Helpers;
using LinyBookStore.Models;
using LinyBookStore.Models.AuthAccount;
using System.Data.Entity;
using CaptchaMvc.Interface;
using CaptchaMvc.HtmlHelpers;
using System.EnterpriseServices.CompensatingResourceManager;

namespace LinyBookStore.Controllers
{
    public class AccountController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();

        //View đăng nhập
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("Login", new { returnUrl = Request.UrlReferrer.ToString() });
            }
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //Code xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModels model, string returnUrl)
        {
            model.Password = Crypto.Hash(model.Password);
            Account account = db.Accounts.SingleOrDefault(m => m.email == model.Email && m.password == model.Password && m.status == "1");
            if (account == null)
            {
                Notification.setNotification3s("Email, mật khẩu không đúng, hoặc tài khoản bị vô hiệu hóa", "error");
                return View(model);
            }
            else
            {
                if (!this.IsCaptchaValid(""))
                {
                    ViewBag.captcha = "Captcha không chính xác";
                }
                else
                {
                    LoggedUserData userData = new LoggedUserData
                    {
                        UserId = account.account_id,
                        Name = account.name,
                        Email = account.email,
                        RoleCode = account.role,
                        Avatar = account.avatar
                    };
                    Notification.setNotification1_5s("Đăng nhập thành công", "success");
                    FormsAuthentication.SetAuthCookie(JsonConvert.SerializeObject(userData), false);
                    if (!String.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }

        //Đăng xuất tài khoản
        public ActionResult Logout(string returnUrl)
        {
            if (String.IsNullOrEmpty(returnUrl) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                return RedirectToAction("Logout", new { returnUrl = Request.UrlReferrer.ToString() });//tạo url khi đăng xuất, đăng xuất thành công thì quay lại trang trước đó
            }
            FormsAuthentication.SignOut();
            Notification.setNotification1_5s("Đăng xuất thành công", "success");
            if (!String.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        // View đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //Code xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModels model, Account account)
        {
            string fail = "";
            string success = "";
            var checkmail = db.Accounts.Any(m => m.email == model.Email);
            if (checkmail)
            {
                fail = "email đã được sử dụng";
                return View();
            }
            account.role = Const.ROLE_MEMBER_CODE; //admin quyền là 0: thành viên quyền là 1 
            account.status = "1";
            account.role = 1;
            account.email = model.Email;
            account.password = Crypto.Hash(model.Password); //mã hóa password
            account.name = model.Name;
            account.phone = model.PhoneNumber;
            account.create_at = DateTime.Now;
            account.create_by = model.Name;
            account.update_at = DateTime.Now;
            account.update_by = model.Name;
            account.avatar = "/Content/Images/logo/favicon.png";
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Accounts.Add(account); // add dữ liệu vào database
            db.SaveChanges(); //lưu dữ liệu vào database
            success = "<script>alert('Đăng ký thành công');</script>";
            ViewBag.Success = success;
            ViewBag.Fail = fail;
            return RedirectToAction("Login", "Account");
        }

        //View cập nhật thông tin cá nhân
        [Authorize] // Đăng nhập mới có thể truy cập
        public ActionResult Editprofile()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Accounts.Where(u => u.account_id == userId).FirstOrDefault();
            if (user != null)
            {
                return View(user);
            }
            return View();
        }

        //Code xử lý cập nhật thông tin cá nhân
        [Authorize]// Đăng nhập mới có thể truy cập
        public JsonResult UpdateProfile(string userName, string phoneNumber)
        {
            bool result = false;
            var userId = User.Identity.GetUserId();
            var account = db.Accounts.Where(m => m.account_id == userId).FirstOrDefault();
            if (account != null)
            {
                account.account_id = userId;
                account.name = userName;
                account.phone = phoneNumber;
                account.update_by = userName;
                account.update_at = DateTime.Now;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //Cập nhật ảnh đại diện
        public JsonResult UpdateAvatar()
        {
            var userId = User.Identity.GetUserId();
            var account = db.Accounts.Where(m => m.account_id == userId).FirstOrDefault();
            HttpPostedFileBase file = Request.Files[0];
            if (file != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                fileName = fileName + extension;
                account.avatar = "/Content/Images/logo/" + fileName;
                file.SaveAs(Path.Combine(Server.MapPath("~/Content/Images/logo/"), fileName));
                db.Configuration.ValidateOnSaveEnabled = false;
                account.update_at = DateTime.Now;
                db.SaveChanges();
            }
            return Json(JsonRequestBehavior.AllowGet);
        }

        //View thay đổi mật khẩu
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //Code xử lý Thay đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModels model)
        {
            if (User.Identity.IsAuthenticated)
            {
                int userID = User.Identity.GetUserId();
                model.NewPassword = Crypto.Hash(model.NewPassword);
                Account account = db.Accounts.FirstOrDefault(m => m.account_id == userID);
                if (model.NewPassword == account.password)
                {
                    Notification.setNotification3s("Mật khẩu mới và cũ không được trùng!", "error");
                    return View(model);
                }
                account.update_at = DateTime.Now;
                account.update_by = User.Identity.GetEmail();
                account.password = model.NewPassword;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                Notification.setNotification3s("Đổi mật khẩu thành công", "success");
                return RedirectToAction("ChangePassword", "Account");
            }
            return View(model);
        }

        //Quản lý sổ địa chỉ
        public ActionResult Address()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userID = User.Identity.GetUserId();
                var address = db.Addresses.Where(m => m.account_id == userID).ToList();
                ViewBag.Check_address = db.Addresses.Where(m => m.account_id == userID).Count();
                ViewBag.ProvincesList = db.Provinces.OrderBy(m => m.province_name).ToList();
                ViewBag.DistrictsList = db.Districts.OrderBy(m => m.type).ThenBy(m => m.district_name).ToList();
                ViewBag.WardsList = db.Wards.OrderBy(m => m.type).ThenBy(m => m.ward_name).ToList();
                return View(address);
            }
            return RedirectToAction("Index", "Home");
        }
        // Thêm mới địa chỉ 
        public ActionResult AddressCreate(Address address)
        {
            bool result = false;
            var userId = User.Identity.GetUserId();
            var checkdefault = db.Addresses.Where(m => m.account_id == userId).ToList();
            var limit_address = db.Addresses.Where(m => m.account_id == userId).ToList();
            try
            {
                if (limit_address.Count() == 4)//tối đa 4 ký tự
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in checkdefault)
                {
                    if (item.isDefault == true && address.isDefault == true)
                    {
                        item.isDefault = false;
                        db.SaveChanges();
                    }
                }
                address.account_id = userId;
                db.Addresses.Add(address);
                db.SaveChanges();
                result = true;
                Notification.setNotification1_5s("Thêm thành công", "success");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Sửa địa chỉ
        [HttpPost]
        public JsonResult AddressEdit(int id, string username, string phonenumber, int province_id, int district_id, int ward_id, string address_content)
        {
            var address = db.Addresses.FirstOrDefault(m => m.address_id == id);
            bool result;
            if (address != null)
            {
                address.province_id = province_id;
                address.accountUserName = username;
                address.accountPhoneNumber = phonenumber;
                address.district_id = district_id;
                address.ward_id = ward_id;
                address.content = address_content;
                address.account_id = User.Identity.GetUserId();
                db.SaveChanges();
                result = true;
            }
            else
            {
                result = false;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Thay đổi địa chỉ mặc định
        public JsonResult DefaultAddress(int id)
        {
            bool result = false;
            var userid = User.Identity.GetUserId();
            var address = db.Addresses.FirstOrDefault(m => m.address_id == id);
            var checkdefault = db.Addresses.ToList();
            if (User.Identity.IsAuthenticated && !address.isDefault == true)
            {
                foreach (var item in checkdefault)
                {
                    if (item.isDefault == true && item.account_id == userid)
                    {
                        item.isDefault = false;
                        db.SaveChanges();
                    }
                }
                address.isDefault = true;
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        //Xóa địa chỉ
        [HttpPost]
        public JsonResult AddressDelete(int id)
        {
            bool result = false;
            try
            {
                var address = db.Addresses.FirstOrDefault(m => m.address_id == id);
                db.Addresses.Remove(address);
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        // Lấy danh sách quận huyện
        public JsonResult GetDistrictsList(int province_id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<District> districtslist = db.Districts.Where(m => m.province_id == province_id).OrderBy(m => m.type).ThenBy(m => m.district_name).ToList();
            return Json(districtslist, JsonRequestBehavior.AllowGet);
        }
        // Lấy danh sách phường xã
        public JsonResult GetWardsList(int district_id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Ward> wardslist = db.Wards.Where(m => m.district_id == district_id).OrderBy(m => m.type).ThenBy(m => m.ward_name).ToList();
            return Json(wardslist, JsonRequestBehavior.AllowGet);
        }

        // Lịch sử mua hàng
        public ActionResult HistoryOrder(int? page)
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("HistoryOrder", GetOrder(page));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        // Chi tiết đơn hàng đã mua
        public ActionResult HistoryOrderDetail(int id)
        {
            List<OrderDetail> order = db.OrderDetails.Where(m => m.order_id == id).ToList();
            ViewBag.Order = db.Orders.FirstOrDefault(m => m.order_id == id);
            ViewBag.OrderID = id;
            if (User.Identity.IsAuthenticated)
            {
                return View(order);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Đánh số trang
        private IPagedList GetOrder(int? page)
        {
            var userId = User.Identity.GetUserId();
            int pageSize = 10;
            int pageNumber = (page ?? 1); //đánh số trang
            var list = db.Orders.Where(m => m.account_id == userId).OrderByDescending(m => m.order_id).ToPagedList(pageNumber, pageSize);
            return list;
        }

        public ActionResult AddAddress()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult UserLogged()
        {
            // Được gọi khi nhấn [Thanh toán]
            return Json(User.Identity.IsAuthenticated, JsonRequestBehavior.AllowGet);
        }

    }
}