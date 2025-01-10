using System.Diagnostics;
using System.Security.Claims;
using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(string searchString, int? page)
        {

            int pageSize = 4; // 每頁顯示 4 筆資料
            int pageNumber = page ?? 1; // 當前頁碼，默認為 1
            // 取得所有商品
            var products = _context.Products.AsNoTracking().AsQueryable();

            // 如果有搜尋字串，過濾商品
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedProducts);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // 從 ClaimsPrincipal 中取得 user 資訊
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
            TempData["SuccessMessage"] = "商品已成功加入購物車！";
            return RedirectToAction("Index", "Home");
        }


        public IActionResult Cart()
        {
            // 從 ClaimsPrincipal 中取得 user 資訊
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
        public IActionResult UpdateCartItemQuantity(IFormCollection form)
        {
            // 從 ClaimsPrincipal 中取得 user 資訊
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // 處理表單中所有的數量變更
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("quantity-"))
                {
                    // 提取 cartItemId
                    var cartItemId = int.Parse(key.Split('-')[1]);
                    var quantity = int.Parse(form[key]);

                    // 查找購物車項目並更新數量
                    var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);
                    if (cartItem != null)
                    {
                        cartItem.Quantity = quantity;
                    }
                }
            }

            _context.SaveChanges(); // 保存所有更改
            return RedirectToAction(nameof(Cart)); // 重定向回購物車頁面
        }


        // 移除購物車商品
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        // 清空購物車
        [HttpPost]
        public IActionResult ClearCart()
        {
            // 從 ClaimsPrincipal 中取得 user 資訊
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