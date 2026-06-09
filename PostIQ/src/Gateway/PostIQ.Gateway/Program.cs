using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
	.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"ocelot.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

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
//app.UseAuthentication();
//app.UseAuthorization();

await app.UseOcelot();

app.Run();