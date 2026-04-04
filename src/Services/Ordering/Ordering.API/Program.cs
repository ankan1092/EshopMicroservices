var builder = WebApplication.CreateBuilder(args);

//Add Services to the Container.
var app = builder.Build();




// configure the HTTP request pipeline.

app.Run();
