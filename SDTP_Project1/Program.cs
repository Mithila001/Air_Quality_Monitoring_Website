using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Repositories;
using SDTP_Project1.Services;
using SDTP_Project1.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the DB context with DI
builder.Services.AddDbContext<AirQualityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection: Register repositories.
builder.Services.AddScoped<ISensorRepository, SensorRepository>();

// Load DevModeOptions from appsettings.json
var initialDevMode = builder.Configuration.GetSection("DevModeOptions").Get<DevModeOptions>()?.Enabled ?? false;
// Register runtime dev-mode state
builder.Services.AddSingleton(new DevModeState { Enabled = initialDevMode });

// Register the hosted background simulation service
builder.Services.AddHostedService<SensorDataSimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
