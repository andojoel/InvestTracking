using Services.SeedCurrencies;

var builder = WebApplication.CreateBuilder(args);

//Configure Services
builder.Services.AddHostedService<BackgroundWorkerService>();

var app = builder.Build();


app.Run();
