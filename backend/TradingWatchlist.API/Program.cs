// backend/TradingWatchlist.API/Program.cs
using Microsoft.EntityFrameworkCore;
using TradingWatchlist.Infrastructure.Data;
using TradingWatchlist.Core.Interfaces;
using TradingWatchlist.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Register Services
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<INoteService, NoteService>();

// Configure HttpClient with User-Agent header for Yahoo Finance
builder.Services.AddHttpClient<IPriceService, PriceService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
// app.UseHttpsRedirection(); // Deaktiviert für Development
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    db.Database.Migrate();
}

app.Run();