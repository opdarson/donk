using donk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization; // 用於密碼雜湊

namespace donk.Controllers
{
    [AllowAnonymous]
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
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Index(Usersmetadata user)
        {
            if (ModelState.IsValid)
            {
                // 根據使用者名稱取得使用者
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);

                if (existingUser != null)
                {
                    // 使用 PasswordHasher 驗證密碼是否正確
                    var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                        existingUser,
                        existingUser.Password, // 資料庫中的雜湊密碼
                        user.Password // 使用者輸入的密碼
                    );

                    if (passwordVerificationResult == PasswordVerificationResult.Success)
                    {
                        // 移除其他裝置的登入會話
                        var existingSession = _context.LoginSession.FirstOrDefault(ls => ls.UserId == existingUser.Id);
                        if (existingSession != null)
                        {
                            _context.LoginSession.Remove(existingSession);
                        }

                        // 生成 GUID 作為 Session ID
                        var sessionId = Guid.NewGuid();

                        // 設置 Cookie 建議30天
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddDays(30)
                        };
                        Response.Cookies.Append("SessionId", sessionId.ToString(), cookieOptions);

                        // 儲存資料至DB Login Session ，DB也記錄使用者相關資訊
                        var newLoginSession = new LoginSession
                        {
                            SessionId = sessionId,
                            UserId = existingUser.Id,
                            Username = existingUser.Username,
                            Email = existingUser.Email,
                            ExpireAt = DateTime.UtcNow.AddDays(30)
                        };
                        _context.LoginSession.Add(newLoginSession);
                        _context.SaveChanges();

                        TempData["SuccessMessage"] = "登入成功!";
                        return RedirectToAction("Index", "Home");
                    }
                }

                // 如果帳號或密碼錯誤，顯示錯誤訊息
                TempData["ErrorMessage"] = "帳號或密碼錯誤！";
            }
            return View(user);
        }



        [Authorize]
        // GET: /Login/Logout
        public IActionResult Logout()
        {
            // 從請求的 Cookie 中取得 "SessionId"
            var sessionId = Request.Cookies["SessionId"];

            // 如果 sessionId 為空或 null，重導至首頁
            if (string.IsNullOrEmpty(sessionId))
            {
                return RedirectToAction("Index");
            }

            // 清空Response的Cookie
            Response.Cookies.Delete("SessionId");

            // 嘗試將 sessionId 轉換為 Guid 型別
            Guid guidSessionId = Guid.Parse(sessionId);

            // 從資料庫中查找對應的登入會話
            var loginSession = _context.LoginSession.FirstOrDefault(x => x.SessionId == guidSessionId);

            // 如果找到登入會話，則將其移除並保存變更
            if (loginSession != null)
            {
                _context.LoginSession.Remove(loginSession);
                _context.SaveChanges();
            }

            // 重導至首頁（登入頁面）
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

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 從當前會話獲取使用者資訊
            var sessionId = Request.Cookies["SessionId"];
            if (string.IsNullOrEmpty(sessionId))
            {
                TempData["ErrorMessage"] = "無效的會話，請重新登入！";
                return RedirectToAction("Index");
            }

            var loginSession = _context.LoginSession.FirstOrDefault(ls => ls.SessionId.ToString() == sessionId);
            if (loginSession == null)
            {
                TempData["ErrorMessage"] = "無效的會話，請重新登入！";
                return RedirectToAction("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == loginSession.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "使用者不存在！";
                return RedirectToAction("Index");
            }

            // 驗證舊密碼
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password, // 資料庫中的雜湊密碼
                model.OldPassword // 使用者輸入的舊密碼
            );

            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "舊密碼不正確！";
                return View(model);
            }

            // 驗證成功，更新新密碼
            user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
            _context.SaveChanges();




            TempData["SuccessMessage"] = "密碼修改成功！請重新登入。";
            return RedirectToAction("Logout");
        }



    }
}