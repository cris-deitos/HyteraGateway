namespace HyteraGateway.Radio.Protocol.RadioServer;

/// <summary>
/// BPG NETRadioServer protocol (TCP port 8000) for client connections
/// This is the protocol used by NETRadioClient.dll to communicate with NETRadioServer
/// </summary>
public class RadioServerProtocol
{
    public const int DEFAULT_PORT = 8000;
    
    public enum ServerCommand : ushort
    {
        // Client registration
        CLIENT_REGISTER = 0x1000,
        CLIENT_REGISTER_ACK = 0x1001,
        CLIENT_UNREGISTER = 0x1002,
        
        // Radio commands from client
        SEND_PTT = 0x2000,
        SEND_GPS_REQUEST = 0x2001,
        SEND_TEXT_MESSAGE = 0x2002,
        SEND_EMERGENCY_ACK = 0x2003,
        
        // Events to client (from radio)
        EVENT_PTT_PRESSED = 0x3000,
        EVENT_PTT_RELEASED = 0x3001,
        EVENT_CALL_START = 0x3002,
        EVENT_CALL_END = 0x3003,
        EVENT_GPS_POSITION = 0x3004,
        EVENT_EMERGENCY_ALERT = 0x3005,
        EVENT_TEXT_MESSAGE = 0x3006,
        EVENT_RADIO_STATUS = 0x3007,
        
        // Voice streaming
        VOICE_FRAME_TO_CLIENT = 0x4000,
        VOICE_FRAME_FROM_CLIENT = 0x4001,
        
        // Keepalive
        KEEPALIVE = 0x5000,
        KEEPALIVE_ACK = 0x5001
    }
}

/// <summary>
/// Packet structure for RadioServer protocol
/// </summary>
public class RadioServerPacket
{
    public ushort Command { get; set; }
    public uint ClientId { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public ushort Sequence { get; set; }
    
    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        writer.Write(Command);
        writer.Write(ClientId);
        writer.Write((ushort)Payload.Length);
        writer.Write(Payload);
        writer.Write(Sequence);
        
        return ms.ToArray();
    }
    
    public static RadioServerPacket FromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);
        
        return new RadioServerPacket
        {
            Command = reader.ReadUInt16(),
            ClientId = reader.ReadUInt32(),
            Payload = reader.ReadBytes(reader.ReadUInt16()),
            Sequence = reader.ReadUInt16()
        };
    }
}
