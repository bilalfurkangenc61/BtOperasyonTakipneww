using BtOperasyonTakip.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Veritabanı bağlantısı (SQL Server veya MySQL fark etmez)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 MVC
builder.Services.AddControllersWithViews();

// 🔹 Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";         // Giriş yapılmadığında yönlenecek sayfa
        options.LogoutPath = "/Auth/Logout";       // Çıkış yolu
        options.AccessDeniedPath = "/Auth/Login";  // Yetkisiz erişim yönlendirmesi
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Remember Me aktifse 7 gün açık kalır
        options.SlidingExpiration = true;          // Her işlemde süre yenilenir
    });

var app = builder.Build();

// 🔹 Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Authentication ve Authorization sırası önemli!
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Varsayılan yönlendirme (Login → Dashboard)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");




app.Run();
