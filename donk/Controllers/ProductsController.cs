using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace donk.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {

        private readonly loginproContext _context;

        public ProductsController(loginproContext context)
        {
            _context = context;
        }




        // 新增商品頁面 (GET)
        public IActionResult Create()
        {
            return View();
        }



        // 新增商品功能 (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductsCreate product)
        {
            if (ModelState.IsValid)
            {

                    // 設定圖片儲存路徑
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName); // 使用 GUID 確保檔名唯一
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // 確保目錄存在
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // 儲存檔案到指定位置
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }

                    // 設定資料庫中的圖片路徑
                    var imagePath = $"/images/products/{fileName}";


                // 儲存商品到資料庫
                var newProduct = new Products
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageData = imagePath
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                // 設置成功訊息
                TempData["SuccessMessage"] = "商品新增成功！";

                return RedirectToAction(nameof(Create));
            }

            return View(product);
        }






    }
}
