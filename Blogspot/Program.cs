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

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string adminEmail = "ADMIN@gmail.com";
    string adminPassword = "Admin@123";
    var existingAdminRole = await roleManager.FindByNameAsync("Admin");

    if(existingAdminRole == null)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
   var existingAdminUser = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdminUser == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

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
