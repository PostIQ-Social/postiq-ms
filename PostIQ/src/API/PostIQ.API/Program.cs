using PostIQ.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;
var services = builder.Services;
{
    Identity.Infrastructure.Extensions.ServiceExtension.AddServiceExtension(services, config);
    Published.Infrastructure.Extension.DbContextExtension.AddDbContextExtension(services, config);
    Published.Infrastructure.Extension.PublishedJobExtension.AddJobExtension(services, config);
    Home.Infrastructure.Extensions.DbContextExtension.AddDbContextExtension(services, config);
    User.Infrastructure.Extensions.DbContextExtension.AddDbContextExtension(services, config);

    services.AddAuthorization();
    services.AddControllers();
    services.AddEndpointsApiExplorer();
}


var app = builder.Build();
{

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        //app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseRateLimiter();

    app.UseMiddleware<JwtAuthorizationMiddleware>();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
