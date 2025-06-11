using WorkerServiceTemplate;
using WorkerServiceTemplate.Models;

var builder = Host.CreateApplicationBuilder(args);

// Register configuration
builder.Services.Configure<AppConfiguration>(
    builder.Configuration.GetSection("AppConfiguration"));

builder.Services.AddHostedService<Worker>();

// Enable Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = Worker.ServiceInternalName;
});

var host = builder.Build();
host.Run();