using System.Security.Claims;
using EStoreAPI.Server.Data;
using EStoreAPI.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

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
builder.Services.AddScoped<IAuthRepo, AuthRepo>();

// services
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IReadOnlyQueryService, ReadOnlyQueryService>();
builder.Services.AddScoped<IUserAccessService, UserAccessService>();

// MCP services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// CORS for local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins("https://localhost:5173")    // vite server port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();    // let the session cookie flow cross-origin in dev
        });
});

// authentication
// using Google OAuth as primary method
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        // challenge via the cookie scheme so unauthenticated API calls get a 401
        // Frontend takes 401 and redirects to login page
        options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        // SPA/XHR callers can't follow login redirects, return status codes instead
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        // access is restricted to a predetermined list of emails (Users table)
        // reject here so a session cookie is never issued for unknown accounts
        options.Events.OnTicketReceived = async context =>
        {
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
            var userAccess = context.HttpContext.RequestServices.GetRequiredService<IUserAccessService>();
            if (!await userAccess.IsEmailAllowedAsync(email))
            {
                context.Response.Redirect("/login?error=denied");
                context.HandleResponse();   // suppress the default cookie sign-in
            }
        };
    });

// authorisation policy
// global authorisation policy, either get in and use everything or don't
builder.Services
    .AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

var app = builder.Build();

// trust and process forwarded headers from a reverse proxy (nginx)
var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaderOptions.KnownProxies.Clear();
forwardedHeaderOptions.KnownIPNetworks.Clear();
app.UseForwardedHeaders(forwardedHeaderOptions);

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /mcp is not proxied by nginx, so it is unreachable from outside the docker network
// the agent is trusted via network isolation rather than credentials
app.MapMcp("/mcp").AllowAnonymous();

// the SPA shell must load for unauthenticated users so the login page can render
app.MapFallbackToFile("/index.html").AllowAnonymous();

// apply ef migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EStoreDbContext>();
    db.Database.Migrate();
    // apply readonly privileges to agent's sql access
    // AgentReadOnlyGrants define the tables allowed
    AgentReadOnlyGrants.Apply(db, app.Configuration, app.Logger);
}

app.Run();
