using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RockFS.Builders;
using RockFS.Data;
using RockFS.Services.Email;
using RockFS.Services.RockFS;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.ConfigureAuth();

builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<RazorRenderer>();

builder.Services.AddTransient<EmailSender>();
builder.Services.AddTransient<StateService>();
builder.Services.AddTransient<RoleService>();
builder.Services.AddTransient<RfsService>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

var app = builder.Build();

var scope = app.Services.CreateScope();

await scope.ServiceProvider.GetService<RoleService>()!
    .InitAsync(scope.ServiceProvider.GetService<RoleManager<IdentityRole>>()!);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();