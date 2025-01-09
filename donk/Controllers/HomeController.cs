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

            return View(pagedProducts);

        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // ���o��e�n�J�� Username
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login"); // ���n�J�h����ܵn�J��
            }

            // �ھ� Username �d�ߨϥΪ̪� UserId
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Index", "Login"); // �p�G�䤣��ϥΪ̡A����ܵn�J��
            }

            var userId = user.Id;

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
            TempData["SuccessMessage"] = "�ӫ~�w���\�[�J�ʪ����I";
            return RedirectToAction("Index" , "Home");
        }


       public IActionResult Cart()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            // �ھ� Username �d�ߨϥΪ̪� UserId
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Index", "Login"); // �p�G�䤣��ϥΪ̡A����ܵn�J��
            }

            var userId = user.Id;

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
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            // �ھ� Username �d�ߨϥΪ̪� UserId
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Index", "Login"); // �p�G�䤣��ϥΪ̡A����ܵn�J��
            }

            var userId = user.Id;

            // �B�z��椤�Ҧ����ƶq�ܧ�
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("quantity-"))
                {
                    // ���� cartItemId
                    var cartItemId = int.Parse(key.Split('-')[1]);
                    var quantity = int.Parse(form[key]);

                    // �d���ʪ������بç�s�ƶq
                    var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId && ci.UserId == userId);
                    if (cartItem != null)
                    {
                        cartItem.Quantity = quantity;
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
            var cartItem = _context.CartItems.SingleOrDefault(ci => ci.Id == cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Cart));
        }

        // �M���ʪ���
        [HttpPost]
        public IActionResult ClearCart()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            // �d���e�ϥΪ�
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // �R���ʪ��������Ҧ�����
            var cartItems = _context.CartItems.Where(ci => ci.UserId == user.Id);
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

