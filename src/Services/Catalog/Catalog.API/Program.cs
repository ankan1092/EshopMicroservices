using Carter;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Add services to the Container.
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce Catalog MicroService API",
        Version = "v1",
        Description = "API documentation for the ECommerce Catalog MicroService API."
    });
});

var app = builder.Build();

//Configure the Http request Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger at the root
    });
}

//Map Carter Endpoints
app.MapCarter();

app.Run();
 