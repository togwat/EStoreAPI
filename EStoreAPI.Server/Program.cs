using EStoreAPI.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db and repo
builder.Services.AddDbContext<EStoreDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebAPIDatabase")));
builder.Services.AddScoped<IEStoreRepo, EStoreRepo>();

// CORS for local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins("https://localhost:5173")    // vite server port
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.UseCors("Frontend");

app.Run();
