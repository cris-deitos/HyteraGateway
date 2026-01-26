using HyteraGateway.Core.Configuration;
using HyteraGateway.Core.Interfaces;
using HyteraGateway.Radio.Services;
using HyteraGateway.Service;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Services.Configure<HyteraGatewayConfig>(
    builder.Configuration.GetSection("HyteraGateway"));

// Add Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "HyteraGateway";
});

// Register application services
builder.Services.AddSingleton<IRadioService, HyteraConnectionService>();

// Add the worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
