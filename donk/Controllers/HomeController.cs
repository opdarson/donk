using System.Diagnostics;
using donk.Models;
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

            int pageSize = 4; // �C����� 8 �����
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



            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }

