using HyteraGateway.Radio.Protocol.Hytera;
using Microsoft.Extensions.Logging;

namespace ProtocolTester;

class Program
{
    // Configuration constants
    private const int DefaultPttHoldDurationMs = 2000;
    
    static async Task<int> Main(string[] args)
    {
        // Parse command line arguments
        var options = ParseArguments(args);
        if (options == null)
        {
            PrintUsage();
            return 1;
        }

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(options.Verbose ? LogLevel.Debug : LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            logger.LogInformation("Protocol Tester - Hytera Gateway");
            logger.LogInformation("Radio IP: {RadioIp}:{Port}", options.RadioIp, options.Port);
            logger.LogInformation("Dispatcher ID: {DispatcherId}", options.DispatcherId);
            logger.LogInformation("");

            // Create connection
            using var connection = new HyteraConnection(
                options.RadioIp,
                options.DispatcherId,
                options.Port
            );

            // Subscribe to events
            connection.PacketReceived += (sender, packet) =>
            {
                logger.LogInformation(">>> Received: {Command} from {SourceId} to {DestId}",
                    packet.Command, packet.SourceId, packet.DestinationId);
                
                if (packet.Payload.Length > 0)
                {
                    logger.LogDebug("    Payload: {Payload} bytes", packet.Payload.Length);
                }
            };

            connection.ConnectionLost += (sender, e) =>
            {
                logger.LogWarning("!!! Connection lost!");
            };

            // Execute command
            bool success = await ExecuteCommand(connection, options, logger);

            return success ? 0 : 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error");
            return 1;
        }
    }

    static async Task<bool> ExecuteCommand(HyteraConnection connection, Options options, ILogger logger)
    {
        switch (options.Command.ToLower())
        {
            case "connect":
                return await TestConnection(connection, logger);

            case "ptt":
                return await TestPtt(connection, options, logger);

            case "gps":
                return await TestGps(connection, options, logger);

            case "sms":
                return await TestSms(connection, options, logger);

            case "interactive":
                return await InteractiveMode(connection, options, logger);

            default:
                logger.LogError("Unknown command: {Command}", options.Command);
                return false;
        }
    }

    static async Task<bool> TestConnection(HyteraConnection connection, ILogger logger)
    {
        logger.LogInformation("Testing connection...");
        
        if (await connection.ConnectAsync())
        {
            logger.LogInformation("✓ Connection successful");
            
            logger.LogInformation("Waiting 5 seconds to receive packets...");
            await Task.Delay(5000);
            
            await connection.DisconnectAsync();
            logger.LogInformation("✓ Disconnected");
            return true;
        }
        else
        {
            logger.LogError("✗ Connection failed");
            return false;
        }
    }

    static async Task<bool> TestPtt(HyteraConnection connection, Options options, ILogger logger)
    {
        logger.LogInformation("Testing PTT...");
        
        if (!await connection.ConnectAsync())
        {
            logger.LogError("✗ Connection failed");
            return false;
        }

        try
        {
            // Press PTT
            logger.LogInformation("Pressing PTT for talkgroup {TargetId}...", options.TargetId);
            await connection.SendPttAsync(options.TargetId, press: true, slot: 0);
            
            await Task.Delay(DefaultPttHoldDurationMs); // Hold for configured duration
            
            // Release PTT
            logger.LogInformation("Releasing PTT...");
            await connection.SendPttAsync(options.TargetId, press: false, slot: 0);
            
            logger.LogInformation("✓ PTT test complete");
            
            await Task.Delay(1000);
            await connection.DisconnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ PTT test failed");
            await connection.DisconnectAsync();
            return false;
        }
    }

    static async Task<bool> TestGps(HyteraConnection connection, Options options, ILogger logger)
    {
        logger.LogInformation("Testing GPS request...");
        
        if (!await connection.ConnectAsync())
        {
            logger.LogError("✗ Connection failed");
            return false;
        }

        try
        {
            logger.LogInformation("Requesting GPS from radio {TargetId}...", options.TargetId);
            await connection.RequestGpsAsync(options.TargetId);
            
            logger.LogInformation("Waiting for GPS response...");
            await Task.Delay(10000); // Wait 10 seconds for response
            
            logger.LogInformation("✓ GPS test complete (check logs for response)");
            
            await connection.DisconnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ GPS test failed");
            await connection.DisconnectAsync();
            return false;
        }
    }

    static async Task<bool> TestSms(HyteraConnection connection, Options options, ILogger logger)
    {
        logger.LogInformation("Testing SMS...");
        
        if (!await connection.ConnectAsync())
        {
            logger.LogError("✗ Connection failed");
            return false;
        }

        try
        {
            string message = options.Message ?? "Test message from Protocol Tester";
            logger.LogInformation("Sending SMS to {TargetId}: {Message}", options.TargetId, message);
            
            await connection.SendTextMessageAsync(options.TargetId, message);
            
            logger.LogInformation("✓ SMS sent");
            
            await Task.Delay(2000);
            await connection.DisconnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "✗ SMS test failed");
            await connection.DisconnectAsync();
            return false;
        }
    }

    static async Task<bool> InteractiveMode(HyteraConnection connection, Options options, ILogger logger)
    {
        logger.LogInformation("Interactive mode - connecting...");
        
        if (!await connection.ConnectAsync())
        {
            logger.LogError("✗ Connection failed");
            return false;
        }

        logger.LogInformation("Connected! Available commands:");
        logger.LogInformation("  ptt <target_id>     - Press and release PTT");
        logger.LogInformation("  gps <target_id>     - Request GPS");
        logger.LogInformation("  sms <target_id> <message> - Send text message");
        logger.LogInformation("  quit                - Exit");
        logger.LogInformation("");

        bool running = true;
        while (running)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            string cmd = parts[0].ToLower();

            try
            {
                switch (cmd)
                {
                    case "ptt":
                        if (parts.Length < 2)
                        {
                            logger.LogWarning("Usage: ptt <target_id>");
                            break;
                        }
                        uint pttTarget = uint.Parse(parts[1]);
                        await connection.SendPttAsync(pttTarget, true);
                        await Task.Delay(DefaultPttHoldDurationMs);
                        await connection.SendPttAsync(pttTarget, false);
                        logger.LogInformation("PTT sent to {Target}", pttTarget);
                        break;

                    case "gps":
                        if (parts.Length < 2)
                        {
                            logger.LogWarning("Usage: gps <target_id>");
                            break;
                        }
                        uint gpsTarget = uint.Parse(parts[1]);
                        await connection.RequestGpsAsync(gpsTarget);
                        logger.LogInformation("GPS request sent to {Target}", gpsTarget);
                        break;

                    case "sms":
                        if (parts.Length < 3)
                        {
                            logger.LogWarning("Usage: sms <target_id> <message>");
                            break;
                        }
                        uint smsTarget = uint.Parse(parts[1]);
                        string smsMessage = string.Join(" ", parts.Skip(2));
                        await connection.SendTextMessageAsync(smsTarget, smsMessage);
                        logger.LogInformation("SMS sent to {Target}", smsTarget);
                        break;

                    case "quit":
                    case "exit":
                        running = false;
                        break;

                    default:
                        logger.LogWarning("Unknown command: {Cmd}", cmd);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Command failed");
            }
        }

        await connection.DisconnectAsync();
        logger.LogInformation("Disconnected");
        return true;
    }

    static Options? ParseArguments(string[] args)
    {
        var options = new Options();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--radio-ip":
                    options.RadioIp = args[++i];
                    break;
                case "--port":
                    options.Port = int.Parse(args[++i]);
                    break;
                case "--dispatcher-id":
                    options.DispatcherId = uint.Parse(args[++i]);
                    break;
                case "--target-id":
                    options.TargetId = uint.Parse(args[++i]);
                    break;
                case "--message":
                    options.Message = args[++i];
                    break;
                case "--verbose":
                case "-v":
                    options.Verbose = true;
                    break;
                case "--help":
                case "-h":
                    return null;
                default:
                    if (!args[i].StartsWith("--"))
                    {
                        options.Command = args[i];
                    }
                    break;
            }
        }

        // Validate required options
        if (string.IsNullOrEmpty(options.RadioIp) || options.DispatcherId == 0)
        {
            return null;
        }

        return options;
    }

    static void PrintUsage()
    {
        Console.WriteLine("Protocol Tester - Hytera Gateway");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  ProtocolTester [command] --radio-ip <ip> --dispatcher-id <id> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  connect         Test connection and keepalive");
        Console.WriteLine("  ptt             Test PTT press/release");
        Console.WriteLine("  gps             Test GPS request");
        Console.WriteLine("  sms             Test text messaging");
        Console.WriteLine("  interactive     Interactive mode");
        Console.WriteLine();
        Console.WriteLine("Required Options:");
        Console.WriteLine("  --radio-ip <ip>         Radio IP address");
        Console.WriteLine("  --dispatcher-id <id>    Dispatcher DMR ID");
        Console.WriteLine();
        Console.WriteLine("Optional Options:");
        Console.WriteLine("  --port <port>           Port (default: 50000)");
        Console.WriteLine("  --target-id <id>        Target radio ID for commands");
        Console.WriteLine("  --message <text>        Message text for SMS");
        Console.WriteLine("  --verbose, -v           Enable verbose logging");
        Console.WriteLine("  --help, -h              Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  ProtocolTester connect --radio-ip 192.168.1.100 --dispatcher-id 9000001");
        Console.WriteLine("  ProtocolTester ptt --radio-ip 192.168.1.100 --dispatcher-id 9000001 --target-id 100 -v");
        Console.WriteLine("  ProtocolTester sms --radio-ip 192.168.1.100 --dispatcher-id 9000001 --target-id 123456 --message \"Hello\"");
        Console.WriteLine("  ProtocolTester interactive --radio-ip 192.168.1.100 --dispatcher-id 9000001");
    }

    class Options
    {
        public string RadioIp { get; set; } = "";
        public int Port { get; set; } = 50000;
        public uint DispatcherId { get; set; }
        public uint TargetId { get; set; } = 100;
        public string? Message { get; set; }
        public string Command { get; set; } = "connect";
        public bool Verbose { get; set; }
    }
}
