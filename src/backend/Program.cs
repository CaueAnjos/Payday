using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models;
using PaydayBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContextPool<ContractContext>(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("ContractContext");
    if (connectionString is null)
    {
        connectionString = Environment.GetEnvironmentVariable("PAYDAY_DB");
    }

    if (connectionString is null)
        throw new NullReferenceException(
            "connectionString for ContractContext was null! ContractContext wasn't set, and PAYDAY_DB env var wasn't as well."
        );

    config.UseNpgsql(connectionString);
});

builder.Services.AddDataProtection();
builder.Services.AddSingleton<CursorService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
