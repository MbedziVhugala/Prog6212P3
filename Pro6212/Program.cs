using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Prog6212.Data;
using Prog6212.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Data Service
builder.Services.AddScoped<IDataService, EFDataService>();

// Authentication setup
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Show detailed errors in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: Order matters for these middleware components
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Configure routes - FIX FOR 404 ERROR
app.UseEndpoints(endpoints =>
{
    // Specific route for Claims controller
    endpoints.MapControllerRoute(
        name: "claims",
        pattern: "Claims/{action=Index}/{id?}",
        defaults: new { controller = "Claims" });

    // Default route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");
});

// Alternative routing approach (comment out the above UseEndpoints and use this if still having issues):
// app.MapControllerRoute(
//     name: "claims",
//     pattern: "Claims/{action=Index}/{id?}",
//     defaults: new { controller = "Claims" });

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();