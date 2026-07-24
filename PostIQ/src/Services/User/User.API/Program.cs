using PostIQ.Core.Shared.Extensions;
using User.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;
var services = builder.Services;

{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDbContextExtension(configuration);
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
