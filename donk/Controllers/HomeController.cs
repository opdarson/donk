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
        public async Task<IActionResult> Index(string searchString, int? page)
        {

            int pageSize = 4; // �C����� 4 �����
            int pageNumber = page ?? 1; // ��e���X�A�q�{�� 1
            // ���o�Ҧ��ӫ~
            var products = from p in _context.Products
                           select p;

            // �p�G���j�M�r��A�L�o�ӫ~
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }
            var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber; // �N�ثe���X�s�J ViewBag�A�ѫe�ݨϥ�

            //if (User.Identity.IsAuthenticated)
            //{
            //    var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //    var userId = int.Parse(userIdString!);
            //    ViewBag.CartItemCount = _context.CartItems.Where(ci => ci.UserId == userId).Sum(ci => ci.Quantity);
            //}
            //else
            //{
            //    ViewBag.CartItemCount = 0;
            //}



            return View(pagedProducts);

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

            //TempData["SuccessMessage"] = "�ӫ~�w���\�[�J�ʪ����I";
            //return RedirectToAction("Index", new { page = currentPage });
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
        public IActionResult UpdateCartItemQuantity(IFormCollection form)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdString!);


            // �B�z��椤�Ҧ����ƶq�ܧ�
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("quantity-"))
                {
                    // ���� cartItemId
                    var cartItemId = int.Parse(key.Split('-')[1]);
                    var quantity = int.Parse(form[key]);

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
                    }
                }
            }

            _context.SaveChanges(); // �O�s�Ҧ����
            return RedirectToAction(nameof(Cart)); // ���w�V�^�ʪ�������
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

