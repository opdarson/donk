using System.Diagnostics;
using System.Security.Claims;
using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace donk.Controllers
{
    public class HomeController : Controller
    {

            private readonly loginproContext _context;

            public HomeController(loginproContext context)
            {
                _context = context;
            }

        public async Task<IActionResult> Index(string searchString, int? page)
        {

            int pageSize = 4; // 每頁顯示 4 筆資料
            int pageNumber = page ?? 1; // 當前頁碼，默認為 1
            // 取得所有商品
            var products = from p in _context.Products
                           select p;

            // 如果有搜尋字串，過濾商品
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }
            var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedProducts);

        }

        // 新增到購物車
        [Authorize]
        [HttpPost]

        public IActionResult AddToCart(int productId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Console.WriteLine(userIdClaim);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("使用者未登入或 ID 無效");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound("商品不存在");
            }

            var cartItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
                _context.Update(cartItem);
            }
            else
            {
                cartItem = new CartItems
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Cart");
        }

        // 顯示購物車頁面
        [Authorize]
        public IActionResult Cart()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("使用者未登入或 ID 無效");
            }

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.Id,
                    ProductName = c.Product.Name,
                    ProductPrice = c.Product.Price,
                    c.Quantity,
                    Total = c.Quantity * c.Product.Price
                })
                .ToList();

            return View(cartItems);
        }








        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }

