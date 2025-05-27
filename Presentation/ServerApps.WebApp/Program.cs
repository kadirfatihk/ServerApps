using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ServerApps.Business.Configuration;
using ServerApps.Business.Usescasess;
using ServerApps.Business.Usescasess.Auth;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.Event;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Business.Usescasess.Task;
using ServerApps.Business.Usescasess.TaskScheduler;
using ServerApps.Persistence.Modal;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IIisService, IisService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddMemoryCache(); // IMemoryCache için

var cs = builder.Configuration.GetConnectionString("DvuDb");
builder.Services.AddDbContext<DvuApplicationAuthenticationDbContext>(options => options.UseSqlServer(cs));

// Cookie Authentication yapýlandýrmasý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();

// Pipeline konfigürasyonu
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
