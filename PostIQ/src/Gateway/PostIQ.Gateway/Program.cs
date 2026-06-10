using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration.AddOcelot(env);
	//.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
	//.AddJsonFile($"ocelot.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSection["SigningKey"] ?? "YourSuperSecretKeyThatIsAtLeast32Character";
var issuer = jwtSection["Issuer"] ?? "PoCMicroservices.Identity";
var audience = jwtSection["Audience"] ?? "PoCMicroservices";

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = issuer,
		ValidAudience = audience,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
		ClockSkew = TimeSpan.Zero
	});

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin();
		policy.AllowAnyMethod();
		policy.AllowAnyHeader();
	});
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();


app.Run();