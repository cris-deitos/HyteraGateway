using HyteraGateway.Core.Configuration;
using HyteraGateway.Core.Interfaces;
using HyteraGateway.Radio.Services;
using HyteraGateway.Service;
using HyteraGateway.Audio.Codecs.Ambe;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Services.Configure<HyteraGatewayConfig>(
    builder.Configuration.GetSection("HyteraGateway"));

// Add Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "HyteraGateway";
});

// Register AMBE codec with graceful fallback
// Try to use mbelib if available, otherwise fallback to placeholder (silence)
builder.Services.AddSingleton<IAmbeCodec>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<MbelibAmbeCodec>>();
    try
    {
        var codec = new MbelibAmbeCodec(logger);
        logger.LogInformation("Using MbelibAmbeCodec for AMBE audio decoding");
        return codec;
    }
    catch (DllNotFoundException ex)
    {
        logger.LogWarning(ex, "mbelib not found, falling back to placeholder codec (audio will be silent). " +
                              "To enable audio decoding, install mbelib. See docs/MBELIB_SETUP.md for instructions.");
        return new AmbeCodec();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize MbelibAmbeCodec, falling back to placeholder codec");
        return new AmbeCodec();
    }
});

// Register application services
builder.Services.AddSingleton<IRadioService, HyteraConnectionService>();

// Add the worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
