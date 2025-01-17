using Microsoft.AspNetCore.Mvc;

namespace donk.Controllers
{
    public class EditProfileController : Controller
    {

        private readonly loginproContext _context;

        public EditProfileController(loginproContext context)
        {
            _context = context;
        }




        public IActionResult Index()
        {
            var username = User.Identity.Name; // 獲取當前登入使用者的名稱
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "用戶不存在！";
                return RedirectToAction("Index", "Home");
            }

            var model = new UsersEditProfile
            {
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult Index(UsersEditProfile model)
        {
            var username = User.Identity.Name; // 獲取當前登入使用者的名稱
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                TempData["ErrorMessage"] = "用戶不存在！";
                return RedirectToAction("Index", "Home");
            }

            // 更新資料（不允許修改 Username 和 Email）
            user.Phone = model.Phone;
            user.Address = model.Address;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "個人資料已更新成功！";

            // 修正這裡的重導向
            return RedirectToAction("Index");
        }


    }
}
