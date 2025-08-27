using Mapster;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Stockr.API.Configuration;
using Stockr.Application.Commands.Categories;
using Stockr.Infrastructure.Context;
using Stockr.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

MapsterConfig.ConfigureMapster(TypeAdapterConfig.GlobalSettings);

// Register MediatR with Application assembly
builder.Services.AddMediatR(config => 
{
    config.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly);
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();