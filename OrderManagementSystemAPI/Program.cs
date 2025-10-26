using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.CQRS;
using OrderManagementSystemAPI.CQRS.Commands;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.CQRS.Queries;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Middleware;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories;
using OrderManagementSystemAPI.Repositories.Interfaces;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    }); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order Management System API",
        Version = "v1",
        Description = "An API for managing products, orders, and reports",
        Contact = new() { Name = "Ivan P" }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddDbContext<OrderManagementContext>(options => options.UseInMemoryDatabase("OrderManagementDb"));
builder.Services.AddMemoryCache();

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// CQRS Dispatchers
builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();
// Command Handlers
builder.Services.AddScoped<ICommandHandler<CreateProductCommand, CreateProductResult>, CreateProductCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProductCommand, UpdateProductResult>, UpdateProductCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteProductCommand, DeleteProductResult>, DeleteProductCommandHandler>();
builder.Services.AddScoped<ICommandHandler<SoftDeleteProductCommand, DeleteProductResult>, SoftDeleteProductCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderCommandHandler>();
// Query Handlers
builder.Services.AddScoped<IQueryHandler<GetAllProductsQuery, GetAllProductsResult>, GetAllProductsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetProductByIdQuery, GetProductByIdResult>, GetProductByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>, GetOrderByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetDailySummaryQuery, GetDailySummaryResult>, GetDailySummaryQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetLowStockProductsQuery, GetLowStockProductsResult>, GetLowStockProductsQueryHandler>();
// Fluent Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API V1");
    c.RoutePrefix = string.Empty;
});
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// Initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
    DataSeeder.SeedData(context);
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }