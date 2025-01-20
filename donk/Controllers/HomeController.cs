using System.Diagnostics;
using System.Security.Claims;
using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace donk.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly loginproContext _context;

        public HomeController(loginproContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        public IActionResult Index(string searchString, string category, int? minPrice, int? maxPrice, string sortOrder, int? page)
        {

            var products = _context.Products.AsQueryable();

            // 搜尋
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }



            // 價格範圍篩選
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice);
            }

            //// 排序
            //if (!string.IsNullOrEmpty(sortOrder))
            //{
            //    if (sortOrder == "price_asc")
            //    {
            //        products = products.OrderBy(p => p.Price);
            //    }
            //    else if (sortOrder == "price_desc")
            //    {
            //        products = products.OrderByDescending(p => p.Price);
            //    }
            //}


            // 分頁
            int pageSize = 3;
            int pageNumber = page ?? 1;

            // 傳遞查詢條件到 View
            ViewData["CurrentFilter"] = searchString;

            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;


            //return PartialView("_ProductListPartial", products.ToPagedList(pageNumber, pageSize));

            return View(products.ToPagedList(pageNumber, pageSize));

        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1, int currentPage = 1)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);



            // 檢查是否已經有相同商品在購物車中
            var existingCartItem = _context.CartItems
                .SingleOrDefault(ci => ci.UserId == userId && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // 如果已經存在相同商品，則增加數量
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // 如果是新商品，則新增一筆資料
                var cartItem = new CartItems
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges(); // 保存變更到資料庫



            // 計算購物車商品總數量
            var cartItemCount = _context.CartItems.Where(ci => ci.UserId == userId).Sum(ci => ci.Quantity);

            return Json(new { success = true, cartItemCount });


        }


        //AJAX 同步用戶購物車數量
        [HttpGet]
        public IActionResult GetCartItemCount()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = int.Parse(userIdString!);

                var cartItemCount = _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .Sum(ci => ci.Quantity);

                return Json(new { count = cartItemCount });
            }

            return Json(new { count = 0 });
        }




        public IActionResult Cart()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);


            // 查詢當前使用者的購物車內容
            var cartItems = _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity,
                    Total = ci.Quantity * ci.Product.Price
                })
                .ToList();

            return View(cartItems);
        }
        // 更新購物車商品數量

        [HttpPost]
        public IActionResult UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // 查找購物車項目
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    // 更新數量
                    cartItem.Quantity = quantity;
                }
                else
                {
                    // 如果數量為 0，刪除該商品
                    _context.CartItems.Remove(cartItem);
                }

                _context.SaveChanges(); // 保存變更
            }



            //重定向回購物車頁面，讓使用者看到更新後的內容
            return RedirectToAction(nameof(Cart));
        }



        // 移除購物車商品
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // 查找指定的購物車項目
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                // 刪除指定項目
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            // 重定向回購物車頁面
            return RedirectToAction(nameof(Cart));
        }

        // 清空購物車
        [HttpPost]
        public IActionResult ClearCart()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);


            // 刪除購物車中的所有項目
            var cartItems = _context.CartItems.Where(ci => ci.UserId == userId);
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction(nameof(Cart));
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}