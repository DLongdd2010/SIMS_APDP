using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
<<<<<<< HEAD
=======
using SIMS_APDP.DesignPatternMinh.Factory;
using SIMS_APDP.DesignPatternMinh.State;
using SIMS_APDP.DesignPatternMinh.Iterator;
using Microsoft.AspNetCore.Authentication.Cookies;
>>>>>>> 774f573 (minh up role student)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
<<<<<<< HEAD
=======

// ??NG KÝ DESIGN PATTERN 
builder.Services.AddSingleton<MenuStateManager>();
builder.Services.AddSingleton<IStudentPageFactory, StudentPageFactory>();
builder.Services.AddSingleton<StudentFeaturesIterator>();

// C?U HÌNH AUTHENTICATION & AUTHORIZATION 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";                    // Trang ??ng nh?p
        options.LogoutPath = "/logout";                  // ??ng xu?t (t?t h?n là dùng route riêng)
        options.AccessDeniedPath = "/Home/AccessDenied"; // Trang t? ch?i truy c?p
        options.ExpireTimeSpan = TimeSpan.FromDays(7);   // Nh? 7 ngày n?u tick "Ghi nh?"
        options.SlidingExpiration = true;                // T? ??ng gia h?n
        options.Cookie.HttpOnly = true;                  // An toàn, JS không ??c ???c
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS thì Secure
        options.Cookie.SameSite = SameSiteMode.Lax;      // Ch?ng CSRF t?t
    });

// Phân quy?n theo Role (r?t c?n n?u dùng [Authorize(Roles = "Admin")]
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
});

// DbContext
>>>>>>> 774f573 (minh up role student)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// C?u hình pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

<<<<<<< HEAD
=======
// B?T BU?C PH?I ?ÚNG TH? T?!
app.UseAuthentication();
>>>>>>> 774f573 (minh up role student)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

<<<<<<< HEAD

app.Run();
=======
app.Run();
>>>>>>> 774f573 (minh up role student)
