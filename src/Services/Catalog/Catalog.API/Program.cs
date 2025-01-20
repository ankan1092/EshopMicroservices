using Carter;

var builder = WebApplication.CreateBuilder(args);

//Add services to the Container.
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

//Configure the Http request Pipeline

//Map Carter Endpoints
app.MapCarter();

app.Run();
 