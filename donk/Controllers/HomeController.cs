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

            int pageSize = 4; // 每頁顯示 8 筆資料
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



            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }

