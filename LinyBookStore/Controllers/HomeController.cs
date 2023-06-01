using LinyBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LinyBookStore.Controllers
{
    public class HomeController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        public ActionResult Index()
        {
            ViewBag.AvgFeedback = db.Comments.ToList();
            ViewBag.HotProduct = db.Products.Where(item => item.status == "1" && item.quantity != "0").OrderByDescending(item => item.buyturn + item.view).Take(8).ToList();
            ViewBag.NewProduct = db.Products.Where(item => item.status == "1" && item.quantity != "0").OrderByDescending(item => item.create_at).Take(8).ToList();
            ViewBag.Laptop = db.Products.Where(item => item.status == "1" && item.type == 1 && item.quantity != "0").OrderByDescending(item => item.buyturn + item.view).Take(8).ToList();
            ViewBag.Accessory = db.Products.Where(item => item.status == "1" && item.type == 2 && item.quantity != "0").OrderByDescending(item => item.buyturn + item.view).Take(8).ToList();
            ViewBag.OrderDetail = db.OrderDetails.ToList();
            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }

    }
}