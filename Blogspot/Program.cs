using Blogspot.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

 var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    //options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
}) .AddEntityFrameworkStores<AppDbContext>();
    
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Set the cookie expiration time
    options.SlidingExpiration = true; // Enable sliding expiration
    //options.LoginPath = "/Auth/Login"; // Set the login path
    //options.AccessDeniedPath = "/Auth/AccessDenied"; // Set the access denied path
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

app.UseAuthorization();

app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Post}/{action=Index}/{id?}")
        .WithStaticAssets();

app.Run();
