using ServerApps.Business.Configuration;
//using ServerApps.Business.IIS;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Business.Usescasess.Task;
using ServerApps.Business.Usescasess.TaskScheduler;
//using ServerApps.Business.Usescasess.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Controller ekle
builder.Services.AddControllers();

// IConfigurationService ve II�sService servislerini ekliyoruz
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IIisService, IisService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// IConfiguration servisini ekliyoruz
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Swagger ekle (test i�in)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Geli�tirme ortam� i�in Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routing ve controller mapping
app.UseAuthorization();
app.MapControllers();

app.Run();
