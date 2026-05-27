using PostIQ.Core.Shared.Extensions;
using Published.Infrastructure.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextExtension(builder.Configuration);
builder.Services.AddJobExtension(builder.Configuration);
builder.Services.AddServiceCollectionExtensions(builder.Configuration,
    typeof(Published.Core.Entities.Job).Assembly,
    typeof(Published.Core.Entities.Repo).Assembly,
    typeof(Published.Infrastructure.Services.MediumService).Assembly,
    typeof(PostIQ.Core.AI.LLM.GeminiLlmClient).Assembly,
    typeof(Published.Application.Models.JobModel).Assembly
);

var app = builder.Build();

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
