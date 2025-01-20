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

            // �j�M
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }



            // ����d��z��
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice);
            }

            //// �Ƨ�
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


            // ����
            int pageSize = 3;
            int pageNumber = page ?? 1;

            // �ǻ��d�߱���� View
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



            // �ˬd�O�_�w�g���ۦP�ӫ~�b�ʪ�����
            var existingCartItem = _context.CartItems
                .SingleOrDefault(ci => ci.UserId == userId && ci.ProductId == productId);

            if (existingCartItem != null)
            {
                // �p�G�w�g�s�b�ۦP�ӫ~�A�h�W�[�ƶq
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // �p�G�O�s�ӫ~�A�h�s�W�@�����
                var cartItem = new CartItems
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges(); // �O�s�ܧ���Ʈw



            // �p���ʪ����ӫ~�`�ƶq
            var cartItemCount = _context.CartItems.Where(ci => ci.UserId == userId).Sum(ci => ci.Quantity);

            return Json(new { success = true, cartItemCount });


        }


        //AJAX �P�B�Τ��ʪ����ƶq
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


            // �d�߷�e�ϥΪ̪��ʪ������e
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
        // ��s�ʪ����ӫ~�ƶq

        [HttpPost]
        public IActionResult UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // �d���ʪ�������
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    // ��s�ƶq
                    cartItem.Quantity = quantity;
                }
                else
                {
                    // �p�G�ƶq�� 0�A�R���Ӱӫ~
                    _context.CartItems.Remove(cartItem);
                }

                _context.SaveChanges(); // �O�s�ܧ�
            }



            //���w�V�^�ʪ��������A���ϥΪ̬ݨ��s�᪺���e
            return RedirectToAction(nameof(Cart));
        }



        // �����ʪ����ӫ~
        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);

            // �d����w���ʪ�������
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);

            if (cartItem != null)
            {
                // �R�����w����
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            // ���w�V�^�ʪ�������
            return RedirectToAction(nameof(Cart));
        }

        // �M���ʪ���
        [HttpPost]
        public IActionResult ClearCart()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);


            // �R���ʪ��������Ҧ�����
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