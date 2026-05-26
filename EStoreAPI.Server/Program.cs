using EStoreAPI.Server.Data;
using EStoreAPI.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// prevent cycles in responses
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// db and repo
builder.Services.AddDbContext<EStoreDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebAPIDatabase")));
builder.Services.AddScoped<IEStoreRepo, EStoreRepo>();

// services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IProblemService, ProblemService>();

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

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

// apply ef migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EStoreDbContext>();
    db.Database.Migrate();
}

app.Run();
