using donk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; // 用於 Session
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
                    // 檢查是否已經有登入會話，如果有則刪除  同時只能登入一個裝置
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
                        Username = user.Username,
                        Email = existingUser.Email,
                        ExpireAt = DateTime.UtcNow.AddDays(30)  
                    };
                    _context.LoginSession.Add(newLoginSession);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "登入成功!";

                    return RedirectToAction("Index", "Checkout");

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
        [Authorize]
        public IActionResult Welcome()
        {
            return View();
        }

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
