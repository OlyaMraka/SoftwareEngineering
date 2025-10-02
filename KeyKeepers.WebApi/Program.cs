using KeyKeepers.DAL.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<KeyKeepersDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Це може допомогти, якщо ваш DAL і API знаходяться в різних проектах
            npgsqlOptions.MigrationsAssembly(typeof(KeyKeepersDbContext).Assembly.FullName);
        });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
