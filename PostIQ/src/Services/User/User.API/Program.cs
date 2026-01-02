using PostIQ.Core.Shared.Extensions;
using User.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;
var services = builder.Services;

{
    services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDbContextExtension(configuration);
    services.AddServiceCollectionExtensions(configuration,
        typeof(User.Core.Entities.Users).Assembly,
        typeof(User.Application.Handlers.GetUserByIdHandler).Assembly,
        typeof(User.Infrastructure.Repositories.UserRepository).Assembly        
    );
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

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
