using BackgammonAPI.API.Mappers;
using BackgammonAPI.Application.Interfaces;
using BackgammonAPI.Application.Services;
using BackgammonAPI.Domain.Services;
using BackgammonAPI.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backgammon API",
        Version = "v1",
        Description = "A REST API for a Backgammon game engine, built with ASP.NET Core " +
                      "using Clean Architecture and Domain-Driven Design. Supports " +
                      "human-vs-human and human-vs-AI play: dice rolling, move validation, " +
                      "hitting and bar re-entry, and turn management.",
        Contact = new OpenApiContact
        {
            Name = "Arina",
            Url = new Uri("https://github.com/<your-username>/BackgammonAPI")
        }
    });

    // Pull endpoint/parameter descriptions from XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<GameEngine>();
builder.Services.AddScoped<GameMapper>();
builder.Services.AddScoped<IGameRepository, InMemoryGameRepository>();
builder.Services.AddScoped<MoveValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
 app.UseSwagger();
 app.UseSwaggerUI();   
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
