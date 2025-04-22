using ServerApps.Business.Configuration;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // MVC servisi eklendi
builder.Services.AddHttpClient(); // HttpClient servisi eklendi

builder.Services.AddScoped<IIisService, IisService>(); // IIisService servisi eklendi
builder.Services.AddScoped<IConfigurationService, ConfigurationService>(); // IConfigurationService servisi eklendi

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // Https yönlendirmesi yapýlacak
app.UseStaticFiles(); // Statik dosyalarýn sunulmasý

app.UseRouting(); // Yönlendirme iþlemleri

app.UseAuthorization(); // Yetkilendirme iþlemleri

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
