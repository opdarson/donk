using donk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace donk.Controllers
{
    public class CheckoutController : Controller
    {


            private readonly loginproContext _context;

            public CheckoutController(loginproContext context)
            {
                _context = context;
            }

            [HttpGet]
        [HttpGet]
        public IActionResult Index()
        {
            // 檢查用戶是否已登入
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            // 查詢當前使用者
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "使用者不存在，請重新登入。";
                return RedirectToAction("Index", "Login");
            }

            // 查詢購物車內容
            var cartItems = _context.CartItems
                .Where(ci => ci.UserId == user.Id)
                .Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    Total = ci.Quantity * ci.Product.Price
                })
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "您的購物車是空的，無法進行結帳。";
                return RedirectToAction("Cart", "Home");
            }

            return View(cartItems); // 顯示結帳頁面
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            // 查詢當前使用者
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "使用者不存在，請重新登入。";
                return RedirectToAction("Index", "Login");
            }

            // 查詢購物車內容並檢查 Product 是否為 null
            var cartItems = _context.CartItems
                .Where(ci => ci.UserId == user.Id)
                .Include(ci => ci.Product) // 載入相關的 Product
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "您的購物車是空的，無法進行結帳。";
                return RedirectToAction("Cart", "Home");
            }

            if (cartItems.Any(ci => ci.Product == null))
            {
                TempData["ErrorMessage"] = "部分商品已不存在，請檢查購物車。";
                return RedirectToAction("Cart", "Home");
            }

            // 計算訂單總金額
            decimal totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // 創建訂單
            var order = new Orders
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            // 將購物車內容存為訂單項目
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItems
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price,
                    Total = cartItem.Quantity * cartItem.Product.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            // 清空購物車
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "訂單已成功建立！";
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        [HttpGet]
        public IActionResult OrderConfirmation(int orderId)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var order = _context.Orders
                .Where(o => o.Id == orderId)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new
                        {
                            oi.Product.Name,
                            oi.Quantity,
                            oi.UnitPrice,
                            oi.Total
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(order); // 顯示訂單確認頁面
        }
        [HttpGet]
        public IActionResult MyOrders()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            // 查詢當前使用者
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["ErrorMessage"] = "使用者不存在，請重新登入。";
                return RedirectToAction("Index", "Login");
            }

            // 查詢使用者所有的訂單
            var orders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new OrderItemViewModel
                        {
                            ProductName = oi.Product.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            Total = oi.Total
                        })
                        .ToList()
                })
                .ToList();

            return View(orders);
        }

    }
}
