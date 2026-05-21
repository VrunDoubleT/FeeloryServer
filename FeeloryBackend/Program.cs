using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Add Database connection
builder.Services.AddDatabase(builder.Configuration);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddApiControllers();

// Customize validation error response format
builder.Services.AddCustomModelValidationResponse();

// Register application services (DI)
builder.Services.AddApplicationServices();

// Register JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register redis
builder.Services.AddRedisCache(builder.Configuration);

// Register email
builder.Services.AddEmailService(builder.Configuration);

// Cloudinary
builder.Services.AddCloudinaryService(builder.Configuration);

// RabbitMQ
builder.Services.AddRabbitMQ(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
// app.UseHttpsRedirection();

// Auto migrate database when app starts
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Global exception middleware
app.UseGlobalException();

// Enable authentication/authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

app.Run();