using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PostIQ.Identity.Data;
using PostIQ.Identity.Middleware;
using PostIQ.Identity.Options;
using PostIQ.Identity.Services;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var signingKey = jwtSection.Get<JwtOptions>()?.SigningKey
    ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
var signingBytes = Encoding.UTF8.GetBytes(signingKey);
if (signingBytes.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 UTF-8 bytes.");

builder.Services.AddSingleton<PasswordHasherService>();
builder.Services.AddSingleton<TotpService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<AuthService>();


builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
//    await db.Database.EnsureCreatedAsync();
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseMiddleware<JwtAuthorizationMiddleware>();

app.UseAuthorization();     

app.MapControllers();

//app.MapControllers().RequireRateLimiting("auth");

app.Run();
