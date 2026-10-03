using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;
using PaydayBackend.Services.Pagination;
using PaydayBackend.Services.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

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

builder.Services.AddRepositories();
builder.Services.AddPagination(options =>
{
    options.MaxPageSize = 25;
    options.DefaultPageSize = 20;
});

if (builder.Environment.IsStaging())
{
    builder.WebHost.UseStaticWebAssets();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// ApiExceptionHandler maps domain exceptions (EntityNotFoundException,
// DuplicateEntityException, InvalidContractStateException, ValidationException, ...)
// to proper status codes. Applied in every environment so API responses are
// consistent regardless of Development/Staging/Production.
app.UseWhen(
    httpContext =>
    {
        return httpContext.Request.Path.StartsWithSegments("/api")
            || httpContext.Request.ContentType == "application/json";
    },
    api =>
    {
        api.UseExceptionHandler();
    }
);

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
