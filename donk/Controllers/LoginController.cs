using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // 用於 Session
namespace donk.Controllers
{
    public class LoginController : Controller
    {
        private readonly loginproContext _context;

        public LoginController(loginproContext context)
        {
            _context = context;
        }
        // GET: /Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        public IActionResult Index(Usersmetadata user)
        {
            if (ModelState.IsValid)
            {
                // 檢查用戶名和密碼是否匹配資料庫中的記錄
                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);

                if (existingUser != null)
                {
                    // 設置 Session
                    HttpContext.Session.SetString("Username", existingUser.Username);




                    TempData["SuccessMessage"] = "登入成功!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // 帳號或密碼錯誤，顯示錯誤訊息
                    TempData["ErrorMessage"] = "帳號或密碼錯誤！";
                }
            }
            return View(user);
        }




        // GET: /Login/Welcome
        //要新增授權屬性
        public IActionResult Welcome()
        {
            // 確認是否存在 Session
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index"); // 若 Session 不存在，重導至登入頁面
            }

            ViewData["Username"] = username;
            return View();
        }

        // GET: /Login/Logout
        public IActionResult Logout()
        {
            // 清除 Session
            HttpContext.Session.Clear();


            return RedirectToAction("Index"); // 重導至登入頁面
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Usersregister newUser)
        {
            if (ModelState.IsValid)
            {
                // 驗證帳號和信箱是否分別已存在於資料庫
                var existingUsername = _context.Users.FirstOrDefault(u => u.Username == newUser.Username);
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email == newUser.Email);

                if (existingUsername != null && existingEmail != null)
                {
                    // 帳號和信箱都存在
                    TempData["ErrorMessage"] = "帳號和信箱已存在，請選擇其他帳號及信箱。";
                    return View(newUser);
                }
                else if (existingEmail != null)
                {
                    // 只有信箱存在
                    TempData["ErrorMessage"] = "信箱已存在，請選擇其他信箱。";
                    return View(newUser);
                }
                else if (existingUsername != null)
                {
                    // 只有帳號存在
                    TempData["ErrorMessage"] = "帳號已存在，請選擇其他帳號。";
                    return View(newUser);
                }

                // 新增新用戶到資料庫
                var userToSave = new Users
                {
                    Username = newUser.Username,
                    Password = newUser.Password,
                    Email = newUser.Email
                };

                _context.Users.Add(userToSave);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "用戶註冊成功！";
                return RedirectToAction("Register");
            }

            // 表單驗證未通過，返回表單頁面
            return View(newUser);
        }
    }
}
