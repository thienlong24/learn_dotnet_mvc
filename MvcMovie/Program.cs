using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using OfficeOpenXml;
using Okta.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MvcMovieContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MvcMovieContext")));

// if (builder.Environment.IsDevelopment())
// {
//     builder.Services.AddDbContext<MvcMovieContext>(options =>
//         options.UseSqlite(builder.Configuration.GetConnectionString("MvcMovieContext")));
// }
// else
// {
//     builder.Services.AddDbContext<MvcMovieContext>(options =>
//         options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionMvcMovieContext")));
// }

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Host.UseSerilog();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OktaDefaults.MvcAuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Use cookies for sign-in
    options.DefaultChallengeScheme = OktaDefaults.MvcAuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Register cookie auth
.AddOktaMvc(new OktaMvcOptions
{
    OktaDomain = builder.Configuration["Okta:Domain"],
    ClientId = builder.Configuration["Okta:ClientId"],
    ClientSecret = builder.Configuration["Okta:ClientSecret"],
    AuthorizationServerId = "default",
    Scope = ["openid", "profile", "email"]
});

ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();