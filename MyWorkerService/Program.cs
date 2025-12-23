using MyWorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MyJob>();

var host = builder.Build();
host.Run();