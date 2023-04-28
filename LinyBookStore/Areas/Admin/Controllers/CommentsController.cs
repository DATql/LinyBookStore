using LinyBookStore.Common.Helpers;
using LinyBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class CommentsController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/Comment
        public ActionResult Index(string search, int? size, int? page)
        {
            var pageSize = (size ?? 15);
            var pageNumber = (page ?? 1);
            ViewBag.search = search;
            var list = from a in db.Comments
                       orderby a.create_at descending
                       select a;
            if (!string.IsNullOrEmpty(search))
            {
                list = from a in db.Comments
                       where a.account_id.ToString().Contains(search)
                       orderby a.create_at descending
                       select a;
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        //phản hồi bình luận
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult ReplyComment(int id, string reply_content, RelyComment reply)
        {
            bool result = false;
            if (User.Identity.IsAuthenticated)
            {
                reply.comment_id = id;
                reply.account_id = User.Identity.GetUserId();
                reply.content = reply_content;
                reply.status = "2";
                reply.create_at = DateTime.Now;
                db.RelyComments.Add(reply);
                db.SaveChanges();
                result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}