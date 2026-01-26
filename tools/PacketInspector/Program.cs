using HyteraGateway.Radio.Protocol.Hytera;
using HyteraGateway.Radio.Protocol.DMR;
using System.Text;

namespace PacketInspector;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "hex":
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: hex command requires hex string");
                    PrintUsage();
                    return;
                }
                AnalyzeHex(args[1]);
                break;

            case "file":
                if (args.Length < 2)
                {
                    Console.WriteLine("Error: file command requires file path");
                    PrintUsage();
                    return;
                }
                AnalyzeFile(args[1]);
                break;

            default:
                Console.WriteLine($"Error: Unknown command '{command}'");
                PrintUsage();
                break;
        }
    }

    static void AnalyzeHex(string hexString)
    {
        try
        {
            // Remove spaces and other formatting
            hexString = hexString.Replace(" ", "").Replace("-", "").Replace(":", "");
            
            // Convert to bytes
            byte[] data = ConvertHexStringToBytes(hexString);
            
            Console.WriteLine("Packet Inspector");
            Console.WriteLine("================");
            Console.WriteLine();
            Console.WriteLine($"Total bytes: {data.Length}");
            Console.WriteLine();
            
            // Print hex dump
            PrintHexDump(data);
            Console.WriteLine();
            
            // Try to parse as Hytera IPSC packet
            TryParseHyteraPacket(data);
            
            // Try to parse as DMR packet
            TryParseDMRPacket(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void AnalyzeFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            byte[] data = File.ReadAllBytes(filePath);
            
            Console.WriteLine("Packet Inspector");
            Console.WriteLine("================");
            Console.WriteLine();
            Console.WriteLine($"File: {filePath}");
            Console.WriteLine($"Total bytes: {data.Length}");
            Console.WriteLine();
            
            PrintHexDump(data);
            Console.WriteLine();
            
            TryParseHyteraPacket(data);
            TryParseDMRPacket(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void PrintHexDump(byte[] data)
    {
        Console.WriteLine("Hex Dump:");
        Console.WriteLine("--------");
        
        for (int i = 0; i < data.Length; i += 16)
        {
            // Offset
            Console.Write($"{i:X4}:  ");
            
            // Hex values
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    Console.Write($"{data[i + j]:X2} ");
                }
                else
                {
                    Console.Write("   ");
                }
                
                if (j == 7)
                {
                    Console.Write(" ");
                }
            }
            
            Console.Write("  ");
            
            // ASCII representation
            for (int j = 0; j < 16 && i + j < data.Length; j++)
            {
                byte b = data[i + j];
                char c = (b >= 32 && b <= 126) ? (char)b : '.';
                Console.Write(c);
            }
            
            Console.WriteLine();
        }
    }

    static void TryParseHyteraPacket(byte[] data)
    {
        Console.WriteLine("Hytera IPSC Packet Analysis:");
        Console.WriteLine("----------------------------");
        
        try
        {
            var packet = HyteraIPSCPacket.FromBytes(data);
            
            Console.WriteLine($"✓ Valid Hytera IPSC packet");
            Console.WriteLine($"  Signature:    PH (0x{HyteraIPSCPacket.SIGNATURE:X4})");
            Console.WriteLine($"  Sequence:     {packet.Sequence}");
            Console.WriteLine($"  Command:      {packet.Command} (0x{(ushort)packet.Command:X4})");
            Console.WriteLine($"  Slot:         {packet.Slot}");
            Console.WriteLine($"  Source ID:    {packet.SourceId}");
            Console.WriteLine($"  Dest ID:      {packet.DestinationId}");
            Console.WriteLine($"  Payload:      {packet.Payload.Length} bytes");
            Console.WriteLine($"  CRC:          0x{packet.Crc:X4}");
            
            if (packet.Payload.Length > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  Payload hex:");
                Console.Write("    ");
                for (int i = 0; i < Math.Min(packet.Payload.Length, 64); i++)
                {
                    Console.Write($"{packet.Payload[i]:X2} ");
                    if ((i + 1) % 16 == 0 && i < packet.Payload.Length - 1)
                    {
                        Console.WriteLine();
                        Console.Write("    ");
                    }
                }
                if (packet.Payload.Length > 64)
                {
                    Console.Write($"... ({packet.Payload.Length - 64} more bytes)");
                }
                Console.WriteLine();
                
                // Try to interpret payload
                if (packet.Command == HyteraCommand.TEXT_MESSAGE_SEND || 
                    packet.Command == HyteraCommand.TEXT_MESSAGE_RECEIVE)
                {
                    try
                    {
                        string message = Encoding.UTF8.GetString(packet.Payload);
                        Console.WriteLine($"  Message text: \"{message}\"");
                    }
                    catch { }
                }
                else if (packet.Command == HyteraCommand.GPS_RESPONSE && packet.Payload.Length >= 16)
                {
                    try
                    {
                        double lat = BitConverter.ToDouble(packet.Payload, 0);
                        double lon = BitConverter.ToDouble(packet.Payload, 8);
                        Console.WriteLine($"  GPS Position: {lat}°, {lon}°");
                    }
                    catch { }
                }
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✗ Not a valid Hytera IPSC packet: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error parsing Hytera packet: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    static void TryParseDMRPacket(byte[] data)
    {
        Console.WriteLine("DMR Packet Analysis:");
        Console.WriteLine("-------------------");
        
        try
        {
            var packet = DMRPacket.FromBytes(data);
            
            bool crcValid = DMRPacket.ValidateCrc(data);
            
            Console.WriteLine($"{(crcValid ? "✓" : "✗")} CRC validation: {(crcValid ? "VALID" : "INVALID")}");
            Console.WriteLine($"  Sync pattern: {BitConverter.ToString(packet.SyncPattern).Replace("-", " ")}");
            Console.WriteLine($"  Slot:         {packet.Slot} (Slot {packet.Slot + 1})");
            Console.WriteLine($"  Color Code:   {packet.ColorCode}");
            Console.WriteLine($"  Type:         {packet.Type}");
            Console.WriteLine($"  Call Type:    {packet.CallType}");
            Console.WriteLine($"  Source ID:    {packet.SourceId}");
            Console.WriteLine($"  Dest ID:      {packet.DestinationId}");
            Console.WriteLine($"  Sequence:     {packet.SequenceNumber}");
            Console.WriteLine($"  Payload:      {packet.Payload.Length} bytes");
            Console.WriteLine($"  CRC:          0x{packet.Crc:X4}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✗ Not a valid DMR packet: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error parsing DMR packet: {ex.Message}");
        }
        
        Console.WriteLine();
    }

    static byte[] ConvertHexStringToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string must have even number of characters");
        }

        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    static void PrintUsage()
    {
        Console.WriteLine("Packet Inspector - Hytera Gateway");
        Console.WriteLine();
        Console.WriteLine("Analyzes raw packet data and attempts to parse as Hytera IPSC or DMR packets.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  PacketInspector hex <hex_string>");
        Console.WriteLine("  PacketInspector file <file_path>");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  hex <hex_string>   Analyze hex string (spaces/dashes optional)");
        Console.WriteLine("  file <file_path>   Analyze binary file");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  PacketInspector hex \"50 48 01 00 00 00 15 00 01 80 00 39 30 00 00 64 00 00 00\"");
        Console.WriteLine("  PacketInspector hex 504801000000150001800039300000640000001234");
        Console.WriteLine("  PacketInspector file captured_packet.bin");
    }
}
