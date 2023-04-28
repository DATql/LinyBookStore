using LinyBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LinyBookStore.Areas.Admin.Controllers
{
    public class DashBoardsController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Admin/DashBoards
        public ActionResult Index()
        {
            ViewBag.Order = db.Orders.ToList();
            ViewBag.OrderDetail = db.OrderDetails.ToList();
            ViewBag.ListOrderDetail = db.OrderDetails.OrderByDescending(m => m.create_at).Take(3).ToList();
            ViewBag.ListOrder = db.Orders.Take(7).ToList();
            return View();
        }
    }
}