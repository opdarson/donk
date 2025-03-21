using donk.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthentication(SessionAuthenticationHandler.AuthenticationType)
    .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>(SessionAuthenticationHandler.AuthenticationType, null);
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

// 添加分散式記憶體快取 (Memory Cache)
builder.Services.AddDistributedMemoryCache();


// 配置 Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設置 Session 過期時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // 必要 Cookie
});





builder.Services.AddDbContext<loginproContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // 啟用 Session 中間件

app.UseAuthentication(); // 啟用身份驗證和授權中介軟體
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
