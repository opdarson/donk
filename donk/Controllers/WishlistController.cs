using donk.Models;
using Microsoft.AspNetCore.Mvc;

namespace donk.Controllers
{
    public class WishlistController : Controller
    {
        private readonly loginproContext _context;

        public WishlistController(loginproContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToWishlist(int productId)
        {
         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingWishlistItem = _context.WishlistItems
                .FirstOrDefault(w => w.ProductId == productId && w.UserId == userId);

            if (existingWishlistItem == null)
            {
                var wishlistItem = new WishlistItems
                {
                    ProductId = productId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.WishlistItems.Add(wishlistItem);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "此商品已在關注清單中！" });
            }
        }

        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var wishlistItems = _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.Product)
                .ToList();

            return View(wishlistItems);
        }


        [HttpPost]
        public IActionResult RemoveFromWishlist(int productId)
        {
              var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // 查找關注清單中的項目
            var wishlistItem = _context.WishlistItems
                .FirstOrDefault(w => w.ProductId == productId && w.UserId == userId);

            if (wishlistItem != null)
            {
                _context.WishlistItems.Remove(wishlistItem); // 從資料庫中移除
                _context.SaveChanges();

                TempData["SuccessMessage"] = "商品已成功從關注清單中移除！";
            }
            else
            {
                TempData["ErrorMessage"] = "無法找到該商品於您的關注清單中。";
            }

            // 返回關注清單頁面
            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult AddToCartFromWishlist(int productId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // 檢查該商品是否存在於購物車中
            var existingCartItem = _context.CartItems
                .FirstOrDefault(ci => ci.UserId == userId && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // 若已存在，增加數量
                existingCartItem.Quantity += 1;
            }
            else
            {
                // 若尚未存在，新增至購物車
                var newCartItem = new CartItems
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(newCartItem);
            }

            // 從關注清單中移除該商品
            var wishlistItem = _context.WishlistItems
                .FirstOrDefault(wi => wi.UserId == userIdString && wi.ProductId == productId);

            if (wishlistItem != null)
            {
                _context.WishlistItems.Remove(wishlistItem);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "商品已成功加入購物車並從關注清單移除！";
            return RedirectToAction("Index");
        }


    }
}

