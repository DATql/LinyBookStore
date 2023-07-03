using LinyBookStore.Models;
using LinyBookStore.Common;
using LinyBookStore.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;


namespace LinyBookStore.Controllers
{
    public class CartController : Controller
    {
        LinyBookStoreEntities db = new LinyBookStoreEntities();
        // GET: Cart
        // Mapping sản phẩm lên view
        // Lấy các sản phẩm trong giỏ hàng từ cookies, xác thực chúng với cơ sở dữ liệu và trả về chúng dưới dạng một bộ chứa danh sách đối tượng để sử dụng trong view
        private Tuple<List<Product>, List<int>> GetCart()
        {
            // check null 
            // Các sản phẩm trong giỏ hàng mà được lưu trữ trong cookies
            var cart = Request.Cookies.AllKeys.Where(c => c.IndexOf("product_") == 0); // Phương thức chia khóa cookie thành hai phần bằng ký tự gạch dưới và lấy giá trị của cookie
            var productIds = new List<int>();
            var quantities = new List<int>();
            var errorProduct = new List<string>();
            var cValue = "";
            // Lấy mã sản phẩm & số lượng trong giỏ hàng
            foreach (var item in cart)
            {
                var tempArr = item.Split('_');
                if (tempArr.Length != 2)
                {
                    //Nếu không lấy được thì xem như sản phẩm đó bị lỗi
                    errorProduct.Add(item);
                    continue;
                }
                cValue = Request.Cookies[item].Value; // Lấy giá trị sản phẩm
                productIds.Add(Convert.ToInt32(tempArr[1])); // Thêm vào mảng sản phẩm trong giỏ hàng
                quantities.Add(Convert.ToInt32(String.IsNullOrEmpty(cValue) ? "0" : cValue)); // Kiểm tra xem số lượng sản phẩm có bằng 0 hay không
                if (cValue == "0")
                {
                    Response.Cookies["product_" + tempArr[1]].Expires = DateTime.Now;
                }
            }
            // Select sản phẩm để hiển thị
            var listProduct = new List<Product>();
            foreach (var id in productIds)
            {
                var product = db.Products.SingleOrDefault(p => p.status == "1" && p.product_id == id);
                if (product != null)
                {
                    listProduct.Add(product);
                }
                else
                {
                    // Trường hợp ko chọn được sản phẩm như trong giỏ hàng
                    // Đánh dấu sản phẩm trong giỏ hàng là sản phẩm lỗi
                    errorProduct.Add("product-" + id);
                    quantities.RemoveAt(productIds.IndexOf(id));
                }
            }
            //Xóa sản phẩm bị lỗi khỏi cart
            foreach (var err in errorProduct)
            {
                Response.Cookies[err].Expires = DateTime.Now.AddDays(-1);
            }
            return new Tuple<List<Product>, List<int>>(listProduct, quantities);//lấy id sản phẩm và số lượng
        }

        //Xem trước giỏ hàng ở bất kì layout nào
        public PartialViewResult PreviewCart()
        {
            var cart = this.GetCart();
            ViewBag.Quans = cart.Item2;
            var listProduct = cart.Item1.ToList();
            return PartialView("PreviewCart", listProduct);
        }

        // Xem thông tin giỏ hàng
        public ActionResult ViewCart()
        {
            var cart = this.GetCart();
            ViewBag.Quans = cart.Item2;
            double discount = 0d;
            var listProduct = cart.Item1.ToList();
            if (Session["Discount"] != null && Session["Discountcode"] != null)
            {
                var code = Session["Discountcode"].ToString();
                var discountupdatequan = db.Discounts.Where(d => d.discount_code == code).FirstOrDefault();
                if (discountupdatequan.quantity == 0 || discountupdatequan.discount_start >= DateTime.Now || discountupdatequan.discount_end <= DateTime.Now)
                {
                    Notification.setNotification3s("Mã giảm giá không thể sử dụng", "error");
                    return View(listProduct);
                }
                discount = Convert.ToDouble(Session["Discount"].ToString());
                Session.Remove("Discount");
                Session.Remove("Discountcode");
                return View(listProduct);
            }
            return View(listProduct);
        }

        // Thanh toán giỏ hàng
        [Authorize] // Kiểm tra xem người dùng đã đăng nhập hay chưa
        public ActionResult Checkout()
        {
            // Lấy thông tin người dùng hiện tại và giỏ hàng của người dùng bằng cách gọi hàm GetCart() đã được định nghĩa
            int userId = User.Identity.GetUserId();
            var user = db.Accounts.SingleOrDefault(u => u.account_id == userId);
            var cart = this.GetCart();
            ViewBag.Quans = cart.Item2;
            ViewBag.ListProduct = cart.Item1.ToList();
            ViewBag.CountAddress = db.Addresses.Where(m => m.account_id == userId).Count();
            ViewBag.ListDistrict = db.Districts.OrderBy(m => m.district_name).ToList();
            ViewBag.ListProvince = db.Provinces.OrderBy(m => m.province_name).ToList();
            ViewBag.ListWard = db.Wards.ToList().OrderBy(m => m.ward_name).ToList();
            ViewBag.ListAddress = db.Addresses.Where(m => m.account_id == userId).OrderByDescending(m => m.isDefault).ToList();
            ViewBag.MyAddress = db.Addresses.FirstOrDefault(u => u.account_id == userId && u.isDefault == true);
            if (cart.Item2.Count < 1)
            {
                return RedirectToAction(nameof(ViewCart));
            }
            var products = cart.Item1;
            double total = 0d;
            double discount = 0d;
            double productPrice = 0d;
            for (int i = 0; i < products.Count; i++)
            {
                var item = products[i];
                productPrice = item.price;
                if (item.Discount != null)
                {
                    if (item.Discount.discount_start < DateTime.Now && item.Discount.discount_end > DateTime.Now)
                    {
                        productPrice = item.price - item.Discount.discount_price;
                    }
                }
                total += productPrice * cart.Item2[i];
            }
            // Áp dụng mã giảm giá
            if (Session["Discount"] != null)
            {
                discount = Convert.ToDouble(Session["Discount"].ToString());
                total -= discount;
            }
            ViewBag.Total = total;
            ViewBag.Discount = discount;
            return View(user);
        }

        // Lưu đơn hàng
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SaveOrder(OrderAddress orderAdress, string note, string emailID, string orderID, string orderItem, string orderDiscount, string orderPrice, string orderTotal, string contentWard, string district, string province)
        {
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN"); // Cấu hình các thuộc tính địa phương như ngôn ngữ, định dạng ngày tháng, tiền tệ và số hóa
                double priceSum = 0;
                string productquancheck = "0";
                if (Session["Discount"] != null && Session["Discountcode"] != null)
                {
                    string check_discount = Session["Discountcode"].ToString();
                    var discountupdatequan = db.Discounts.Where(d => d.discount_code == check_discount).SingleOrDefault();
                    //khi mua hàng có sử dụng mã giảm giá và đặt hàng thành công thì trừ số lượng mã giảm giá
                    if (discountupdatequan.quantity == 0 || discountupdatequan.discount_start >= DateTime.Now || discountupdatequan.discount_end <= DateTime.Now)
                    {
                        Notification.setNotification3s("Mã giảm giá không thể sử dụng", "error");
                        return RedirectToAction("ViewCart", "Cart");
                    }
                    else
                    {
                        int newquantity = (discountupdatequan.quantity - 1);
                        discountupdatequan.quantity = newquantity;
                    }
                }
                orderAdress.timesEdit = 0;
                db.OrderAddresses.Add(orderAdress);
                var cart = this.GetCart();
                var listProduct = new List<Product>();
                var order = new Order()
                {
                    account_id = User.Identity.GetUserId(),
                    create_at = DateTime.Now,
                    create_by = User.Identity.GetName(),
                    status = "1",
                    order_note = Request.Form["OrderNote"].ToString(),
                    delivery_id = 1,
                    order_address_id = orderAdress.orderAddress_id,
                    order_date = DateTime.Now,
                    update_at = DateTime.Now,
                    payment_id = 1,
                    update_by = User.Identity.GetName(),
                    total = Convert.ToDouble(TempData["Total"])
                };
                for (int i = 0; i < cart.Item1.Count; i++)
                {
                    var item = cart.Item1[i];
                    var _price = item.price;
                    if (item.Discount != null)
                    {
                        if (item.Discount.discount_start < DateTime.Now && item.Discount.discount_end > DateTime.Now)
                        {
                            _price = item.price - item.Discount.discount_price;
                        }
                    }
                    order.OrderDetails.Add(new OrderDetail
                    {
                        create_at = DateTime.Now,
                        create_by = User.Identity.GetName(),
                        discount_id = item.discount_id,
                        genre_id = item.genre_id,
                        price = _price,
                        product_id = item.product_id,
                        quantity = cart.Item2[i],
                        status = "1",
                        update_at = DateTime.Now,
                        update_by = User.Identity.GetName(),
                        transection = "transection"
                    });
                    // Xóa cart
                    Response.Cookies["product_" + item.product_id].Expires = DateTime.Now.AddDays(-10);
                    // Thay đổi số lượng và số lượt mua của product 
                    var product = db.Products.SingleOrDefault(p => p.product_id == item.product_id);
                    productquancheck = product.quantity;
                    product.buyturn += cart.Item2[i];
                    product.quantity = (Convert.ToInt32(product.quantity ?? "0") - cart.Item2[i]).ToString();
                    listProduct.Add(product);
                    priceSum += (_price * cart.Item2[i]);
                    orderItem += "<tr style='margin'> <td align='left' width='75%' style=' padding: 6px 12px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;' >" +
                                product.product_name + "</td><td align='left' width='25%' style=' padding: 6px 12px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; ' >" + product.price.ToString("#,0₫", culture.NumberFormat) + "</td> </tr>";
                }
                // Thêm dữ liệu vào table
                if (productquancheck.Trim() != "0")
                {
                    db.Orders.Add(order);
                }
                else
                {
                    Notification.setNotification3s("Sản phẩm đã hết hàng", "error");
                    return RedirectToAction("ViewCart", "Cart");
                }
                db.Configuration.ValidateOnSaveEnabled = false;

                await db.SaveChangesAsync();
                Notification.setNotification3s("Đặt hàng thành công", "success");
                Session.Remove("Discount");
                Session.Remove("Discountcode");
                emailID = User.Identity.GetEmail();
                orderID = order.order_id.ToString();
                orderDiscount = (priceSum + 30000 - order.total).ToString("#,0₫", culture.NumberFormat);
                orderPrice = priceSum.ToString("#,0₫", culture.NumberFormat);
                orderTotal = order.total.ToString("#,0₫", culture.NumberFormat);
                Notification.setNotification3s("Đặt hàng thành công", "success");
                return RedirectToAction("HistoryOrder", "Account");
            }
            catch
            {
                Notification.setNotification3s("Lỗi! đặt hàng không thành công", "error");
                return RedirectToAction("Checkout", "Cart");
            }
        }

        //Áp dụng mã giảm giá
        public ActionResult UseDiscountCode(string code)
        {
            var discount = db.Discounts.SingleOrDefault(d => d.discount_code == code);
            if (discount != null)
            {
                if (discount.discount_start < DateTime.Now && discount.discount_end > DateTime.Now && discount.quantity != 0)
                {
                    Session["Discountcode"] = discount.discount_code;
                    Session["Discount"] = discount.discount_price;
                    return Json(new { success = true, discountPrice = discount.discount_price }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false, discountPrice = 0 }, JsonRequestBehavior.AllowGet);
        }

       
    }
}