using HyteraGateway.Api.Hubs;
using HyteraGateway.Audio.Codecs.Ambe;
using HyteraGateway.Audio.Services;
using HyteraGateway.Audio.Streaming;
using HyteraGateway.Core.Configuration;
using HyteraGateway.Core.Interfaces;
using HyteraGateway.Data.Repositories;
using HyteraGateway.Radio.Services;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<HyteraGatewayConfig>(
    builder.Configuration.GetSection("HyteraGateway"));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "HyteraGateway API",
        Version = "v1",
        Description = "REST API for interfacing with Hytera MD785i DMR radios",
        Contact = new()
        {
            Name = "HyteraGateway Project"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add SignalR for real-time events
builder.Services.AddSignalR();

// Register application services
builder.Services.AddSingleton<IRadioService, HyteraConnectionService>();
builder.Services.AddScoped<TransmissionRepository>();
builder.Services.AddScoped<PositionRepository>();

// Register audio services
builder.Services.AddSingleton<IAmbeCodec, MbelibAmbeCodec>();
builder.Services.AddSingleton<AudioStreamManager>();
builder.Services.AddSingleton<AudioCaptureService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HyteraGateway API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Map SignalR hubs
app.MapHub<RadioEventsHub>("/hubs/radio-events");
app.MapHub<AudioHub>("/hubs/audio");

app.Run();
