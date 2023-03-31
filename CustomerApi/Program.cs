using CustomerApi.Data;
using CustomerApi.Infrastructure;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using SharedModels;

var builder = WebApplication.CreateBuilder(args);

// Base URL for the product service when the solution is executed using docker-compose.
// The product service (running as a container) listens on this URL for HTTP requests
// from other services specified in the docker compose file (which in this solution is
// the order service).
string productServiceBaseUrl = "http://productapi/products/";


// RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
// Remember to replace this connectionstring with your own.
string cloudAMQPConnectionString = "host=rabbitmq";

// Add services to the container.

builder.Services.AddDbContext<CustomerApiContext>(opt => opt.UseInMemoryDatabase("CustomersDb"));

// Register repositories for dependenct injection
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();


// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new
    ProductServiceGateway(productServiceBaseUrl));

// Regiser email service for dependency injection
builder.Services.AddScoped<IEmailService, EmailServiceStub>();

// Register CustomerConverter for dependency injection
builder.Services.AddSingleton<IConverter<Customer, CustomerDto>, CustomerConverter>();

// Register MessagePublisher (a messaging gateway) for dependency injection
builder.Services.AddSingleton<IMessagePublisher>(new
    MessagePublisher(cloudAMQPConnectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Initialize the database.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<CustomerApiContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

// Create a message listener in a separate thread.
Console.WriteLine("Started listening program version after push");
Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAMQPConnectionString).Start());
//app.UseHttpsRedirection();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
