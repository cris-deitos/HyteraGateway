using System.Buffers.Binary;
using System.Text;

namespace HyteraGateway.Radio.Protocol.Hytera;

/// <summary>
/// Represents a Hytera IP Site Connect (IPSC) protocol packet
/// </summary>
public class HyteraIPSCPacket
{
    /// <summary>
    /// Packet signature "PH" (0x5048)
    /// </summary>
    public const ushort SIGNATURE = 0x5048;

    /// <summary>
    /// Packet sequence number
    /// </summary>
    public uint Sequence { get; set; }

    /// <summary>
    /// Command code
    /// </summary>
    public HyteraCommand Command { get; set; }

    /// <summary>
    /// Timeslot (0 = Slot 1, 1 = Slot 2)
    /// </summary>
    public byte Slot { get; set; }

    /// <summary>
    /// Source DMR ID
    /// </summary>
    public uint SourceId { get; set; }

    /// <summary>
    /// Destination DMR ID
    /// </summary>
    public uint DestinationId { get; set; }

    /// <summary>
    /// Payload data
    /// </summary>
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// CRC checksum
    /// </summary>
    public ushort Crc { get; set; }

    /// <summary>
    /// Validates a packet without parsing it
    /// </summary>
    /// <param name="data">Raw packet bytes</param>
    /// <returns>True if packet is valid</returns>
    public static bool IsValidPacket(byte[] data)
    {
        if (data == null || data.Length < 8)
            return false;
        
        // Validate signature "PH" (0x50 0x48)
        if (data[0] != 0x50 || data[1] != 0x48)
            return false;
        
        // Validate length field matches actual data length
        ushort declaredLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6, 2));
        if (declaredLength != data.Length)
            return false;
        
        // Validate CRC if packet is long enough
        if (data.Length >= 21) // Minimum packet with CRC
        {
            // Extract CRC from last 2 bytes
            ushort declaredCrc = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(data.Length - 2, 2));
            ushort calculatedCrc = CalculateCrc(data, 0, data.Length - 2);
            
            if (declaredCrc != calculatedCrc)
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Parse raw bytes into a HyteraIPSCPacket
    /// </summary>
    /// <param name="data">Raw packet bytes</param>
    /// <returns>Parsed packet</returns>
    /// <exception cref="InvalidDataException">Thrown if packet is invalid</exception>
    public static HyteraIPSCPacket FromBytes(byte[] data)
    {
        if (!IsValidPacket(data))
        {
            throw new InvalidDataException("Invalid packet: signature, length, or CRC mismatch");
        }

        if (data.Length < 19)
        {
            throw new ArgumentException("Packet too small", nameof(data));
        }

        int offset = 0;

        // Signature (2 bytes)
        ushort signature = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2));
        if (signature != SIGNATURE)
        {
            throw new ArgumentException($"Invalid signature: expected 0x{SIGNATURE:X4}, got 0x{signature:X4}");
        }
        offset += 2;

        var packet = new HyteraIPSCPacket();

        // Sequence (4 bytes, little-endian)
        packet.Sequence = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
        offset += 4;

        // Length (2 bytes, little-endian)
        ushort length = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
        offset += 2;

        // Command (2 bytes, little-endian)
        packet.Command = (HyteraCommand)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));
        offset += 2;

        // Slot (1 byte)
        packet.Slot = data[offset++];

        // Source ID (4 bytes, little-endian)
        packet.SourceId = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
        offset += 4;

        // Destination ID (4 bytes, little-endian)
        packet.DestinationId = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
        offset += 4;

        // Payload (remaining bytes minus 2 for CRC)
        int payloadLength = data.Length - offset - 2;
        if (payloadLength > 0)
        {
            packet.Payload = new byte[payloadLength];
            Array.Copy(data, offset, packet.Payload, 0, payloadLength);
            offset += payloadLength;
        }

        // CRC (2 bytes, little-endian)
        packet.Crc = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset, 2));

        return packet;
    }

    /// <summary>
    /// Serialize the packet to bytes
    /// </summary>
    /// <returns>Raw packet bytes</returns>
    public byte[] ToBytes()
    {
        // Calculate total length: Header (19 bytes) + Payload + CRC (2 bytes)
        int totalLength = 19 + Payload.Length + 2;
        byte[] buffer = new byte[totalLength];
        int offset = 0;

        // Signature "PH" (2 bytes, big-endian)
        BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset, 2), SIGNATURE);
        offset += 2;

        // Sequence (4 bytes, little-endian)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), Sequence);
        offset += 4;

        // Length (2 bytes, little-endian) - this is the total packet length
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), (ushort)totalLength);
        offset += 2;

        // Command (2 bytes, little-endian)
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), (ushort)Command);
        offset += 2;

        // Slot (1 byte)
        buffer[offset++] = Slot;

        // Source ID (4 bytes, little-endian)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), SourceId);
        offset += 4;

        // Destination ID (4 bytes, little-endian)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), DestinationId);
        offset += 4;

        // Payload
        if (Payload.Length > 0)
        {
            Array.Copy(Payload, 0, buffer, offset, Payload.Length);
            offset += Payload.Length;
        }

        // Calculate and write CRC
        ushort calculatedCrc = CalculateCrc(buffer, 0, offset);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), calculatedCrc);
        Crc = calculatedCrc;

        return buffer;
    }

    /// <summary>
    /// Calculate CRC-CCITT checksum
    /// </summary>
    private static ushort CalculateCrc(byte[] data, int offset, int length)
    {
        const ushort polynomial = 0x1021;
        ushort crc = 0xFFFF;

        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ polynomial);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return crc;
    }

    #region Builder Methods

    /// <summary>
    /// Creates a login packet
    /// </summary>
    public static HyteraIPSCPacket CreateLogin(uint dispatcherId, byte[] authData)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.LOGIN,
            SourceId = dispatcherId,
            Payload = authData
        };
    }

    /// <summary>
    /// Creates a keepalive packet
    /// </summary>
    public static HyteraIPSCPacket CreateKeepalive(uint dispatcherId)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.KEEPALIVE,
            SourceId = dispatcherId
        };
    }

    /// <summary>
    /// Creates a PTT press packet
    /// </summary>
    public static HyteraIPSCPacket CreatePttPress(uint sourceId, uint destinationId, byte slot = 0)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.PTT_PRESS,
            SourceId = sourceId,
            DestinationId = destinationId,
            Slot = slot
        };
    }

    /// <summary>
    /// Creates a PTT release packet
    /// </summary>
    public static HyteraIPSCPacket CreatePttRelease(uint sourceId, uint destinationId, byte slot = 0)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.PTT_RELEASE,
            SourceId = sourceId,
            DestinationId = destinationId,
            Slot = slot
        };
    }

    /// <summary>
    /// Creates a GPS request packet
    /// </summary>
    public static HyteraIPSCPacket CreateGpsRequest(uint sourceId, uint targetId)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.GPS_REQUEST,
            SourceId = sourceId,
            DestinationId = targetId
        };
    }

    /// <summary>
    /// Creates a text message packet
    /// </summary>
    public static HyteraIPSCPacket CreateTextMessage(uint sourceId, uint destinationId, string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.TEXT_MESSAGE_SEND,
            SourceId = sourceId,
            DestinationId = destinationId,
            Payload = messageBytes
        };
    }

    /// <summary>
    /// Creates a disconnect packet
    /// </summary>
    public static HyteraIPSCPacket CreateDisconnect(uint dispatcherId)
    {
        return new HyteraIPSCPacket
        {
            Command = HyteraCommand.DISCONNECT,
            SourceId = dispatcherId
        };
    }

    #endregion
}
