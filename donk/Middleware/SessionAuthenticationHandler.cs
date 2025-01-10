﻿using donk.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

// 自訂驗證選項類別
public class SessionAuthenticationOptions : AuthenticationSchemeOptions
{
}

public class SessionAuthenticationHandler : AuthenticationHandler<SessionAuthenticationOptions>
{
    public static readonly string AuthenticationType = "SessionIdAuth";

    private readonly loginproContext _context;


    public SessionAuthenticationHandler(
        loginproContext context,
        IOptionsMonitor<SessionAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 取得使用者cookie sessionId
        var sessionId = Context.Request.Cookies["SessionId"];

        if (string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.Fail("SessionId 驗證失敗");
        }

        // 驗證sessionId是否有效、是否過期
        var loginSession = await _context.LoginSession.FirstOrDefaultAsync(x => x.SessionId == Guid.Parse(sessionId));
        if (loginSession == null || loginSession.ExpireAt < DateTime.Now)
        {
            return AuthenticateResult.Fail("SessionId 驗證失敗");
        }

        // 通過驗證，建立使用者身份
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, loginSession.UserId.ToString()),
            new Claim(ClaimTypes.Name, loginSession.Username),
            new Claim(ClaimTypes.Email, loginSession.Email),
        };

        // 建立身份驗證票據
        var identity = new ClaimsIdentity(claims, AuthenticationType);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationType);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Console.WriteLine("驗證失敗處理");

        // 設定狀態碼為401
        Context.Response.StatusCode = 401;

        // 清除目前的回應內容
        Context.Response.ContentLength = null;
        Context.Response.Headers.Clear();

        // 重定向到登入頁面
        Context.Response.Redirect("/Login/Index");

        await Task.CompletedTask;
    }
}