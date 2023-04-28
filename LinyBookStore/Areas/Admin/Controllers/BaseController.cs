using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        // GET: Admin/Base
        public BaseController() //Kiểm tra xem người dùng đã được xác thực (authenticated) hay chưa
        {
            if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                //Nếu người dùng chưa được xác thực, phương thức sẽ chuyển hướng trình duyệt của người dùng đến trang đăng nhập
                System.Web.HttpContext.Current.Response.Redirect("~/Account/Login");
            }
            else
            {
                if (System.Web.HttpContext.Current.User.IsInRole("Member"))
                {
                    //Nếu người dùng đã được xác thực, phương thức sẽ kiểm tra xem người dùng có thuộc nhóm quyền "Member" hay không. Nếu có, phương thức sẽ chuyển hướng trình duyệt đến trang chủ của người dùng
                    System.Web.HttpContext.Current.Response.Redirect("~/Home/Index");
                }
            }
        }

        //đăng xuất admin quay về trang chủ
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/Home/Index");
        }

        //chuyển từ trang admin sang trang thông tin cá nhân
        public ActionResult ViewProfile()
        {
            return Redirect("~/Account/Editprofile");
        }

        //chuyển từ trang admin sang trang chủ
        public ActionResult BackToHome()
        {
            return Redirect("~/Home/Index");
        }
    }
}