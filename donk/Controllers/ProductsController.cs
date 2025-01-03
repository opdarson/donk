using donk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace donk.Controllers
{
    public class ProductsController : Controller
    {

        private readonly loginproContext _context;

        public ProductsController(loginproContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        // 新增商品頁面 (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 新增商品功能 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Products product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 設定圖片儲存路徑
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName); // 使用 GUID 確保檔名唯一
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // 確保目錄存在
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // 儲存檔案到指定位置
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // 設定資料庫中的圖片路徑
                    product.ImageData = $"/images/products/{fileName}";
                }

                // 儲存商品到資料庫
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 設置成功訊息
                TempData["SuccessMessage"] = "商品新增成功！";

                return RedirectToAction(nameof(Create));
            }

            return View(product);
        }





    }
}
