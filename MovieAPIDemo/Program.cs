using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MovieAPIDemo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI setup for development
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure EF Core with the connection string from appsettings.json
builder.Services.AddDbContext<MovieDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Configure AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure serving of static files from a physical path
app.UseStaticFiles(new StaticFileOptions
{
    // Make sure the directory exists and has proper permissions
    FileProvider = new PhysicalFileProvider(@"D:\Uploads"),
    RequestPath = "/StaticFiles"  // Static files will be accessible under /StaticFiles path
});

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

app.UseCors("AllowAll");
// Use authorization middleware (if required)
app.UseAuthorization();

// Map controllers to endpoints
app.MapControllers();

// Run the application
app.Run();
