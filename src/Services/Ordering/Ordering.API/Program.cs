using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//Add Services to the Container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();




var app = builder.Build();

// configure the HTTP request pipeline.

app.Run();
