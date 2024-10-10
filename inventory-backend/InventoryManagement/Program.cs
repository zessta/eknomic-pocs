using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using InventoryManagement.Infrastructure.Services;
using InventoryManagement.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//postgresql configuration
builder.Services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("InventoryConnection")));

//Services registration
builder.Services.AddScoped<ITransitService, TransitService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

//Repositories registration
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<ITransitRepository, TransitRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<InventoryDbContext>();
//    //context.Database.Migrate(); // Ensure database is created and migrations are applied
//    context.SeedData(); // Call the seed data method
//}

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
