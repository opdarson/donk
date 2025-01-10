using donk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // 用於密碼雜湊

namespace donk.Controllers
{
    public class LoginController : Controller
    {
        private readonly loginproContext _context;
        private readonly PasswordHasher<Users> _passwordHasher; // 密碼雜湊工具

        public LoginController(loginproContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Users>();
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
                // 查詢用戶名
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    // 驗證密碼
                    var result = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, user.Password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        // 設置 Session
                        HttpContext.Session.SetString("Username", existingUser.Username);
                        TempData["SuccessMessage"] = "登入成功!";
                        return RedirectToAction("Index", "Home");
                    }
                }

                // 登入失敗
                TempData["ErrorMessage"] = "帳號或密碼錯誤！";
            }
            return View(user);
        }

        // GET: /Login/Welcome
        public IActionResult Welcome()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index");
            }

            ViewData["Username"] = username;
            return View();
        }

        // GET: /Login/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // GET: /Login/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Login/Register
        [HttpPost]
        public IActionResult Register(Usersregister newUser)
        {
            if (ModelState.IsValid)
            {
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

                // 雜湊密碼
                var userToSave = new Users
                {
                    Username = newUser.Username,
                    Password = _passwordHasher.HashPassword(null, newUser.Password), // 密碼雜湊
                    Email = newUser.Email
                };

                _context.Users.Add(userToSave);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "用戶註冊成功！";
                return RedirectToAction("Register");
            }
            return View(newUser);
        }
    }
}
