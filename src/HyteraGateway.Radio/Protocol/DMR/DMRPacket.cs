using System.Buffers.Binary;

namespace HyteraGateway.Radio.Protocol.DMR;

/// <summary>
/// Represents a DMR protocol packet based on ETSI TS 102 361 standard
/// </summary>
public class DMRPacket
{
    /// <summary>
    /// DMR sync pattern (6 bytes)
    /// </summary>
    public byte[] SyncPattern { get; set; } = new byte[6];

    /// <summary>
    /// Timeslot (0 = Slot 1, 1 = Slot 2)
    /// </summary>
    public byte Slot { get; set; }

    /// <summary>
    /// Color code (0-15)
    /// </summary>
    public byte ColorCode { get; set; }

    /// <summary>
    /// Packet type
    /// </summary>
    public PacketType Type { get; set; }

    /// <summary>
    /// Call type (group/private/all)
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// Source DMR ID (32-bit)
    /// </summary>
    public uint SourceId { get; set; }

    /// <summary>
    /// Destination DMR ID (32-bit)
    /// </summary>
    public uint DestinationId { get; set; }

    /// <summary>
    /// Packet sequence number
    /// </summary>
    public ushort SequenceNumber { get; set; }

    /// <summary>
    /// Packet timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Payload data
    /// </summary>
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// CRC-CCITT checksum
    /// </summary>
    public ushort Crc { get; set; }

    /// <summary>
    /// Parse a raw packet into a DMRPacket object
    /// </summary>
    /// <param name="rawData">Raw packet bytes</param>
    /// <returns>Parsed DMR packet</returns>
    /// <exception cref="ArgumentException">Thrown if packet is too small or invalid</exception>
    public static DMRPacket FromBytes(byte[] rawData)
    {
        if (rawData.Length < 20)
        {
            throw new ArgumentException("Packet too small", nameof(rawData));
        }

        var packet = new DMRPacket();
        int offset = 0;

        // Sync pattern (6 bytes)
        Array.Copy(rawData, offset, packet.SyncPattern, 0, 6);
        offset += 6;

        // Slot (1 byte)
        packet.Slot = rawData[offset++];

        // Color code and packet type (1 byte combined)
        byte ccAndType = rawData[offset++];
        packet.ColorCode = (byte)((ccAndType >> 4) & 0x0F);
        packet.Type = (PacketType)(ccAndType & 0x0F);

        // Source ID (4 bytes, little-endian)
        packet.SourceId = BinaryPrimitives.ReadUInt32LittleEndian(rawData.AsSpan(offset, 4));
        offset += 4;

        // Destination ID (4 bytes, little-endian)
        packet.DestinationId = BinaryPrimitives.ReadUInt32LittleEndian(rawData.AsSpan(offset, 4));
        offset += 4;

        // Call type (1 byte)
        packet.CallType = (CallType)rawData[offset++];

        // Sequence number (2 bytes, little-endian)
        packet.SequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(rawData.AsSpan(offset, 2));
        offset += 2;

        // Payload (remaining bytes minus 2 for CRC)
        int payloadLength = rawData.Length - offset - 2;
        if (payloadLength > 0)
        {
            packet.Payload = new byte[payloadLength];
            Array.Copy(rawData, offset, packet.Payload, 0, payloadLength);
            offset += payloadLength;
        }

        // CRC (2 bytes, little-endian)
        packet.Crc = BinaryPrimitives.ReadUInt16LittleEndian(rawData.AsSpan(offset, 2));

        return packet;
    }

    /// <summary>
    /// Serialize the packet to bytes for transmission
    /// </summary>
    /// <returns>Raw packet bytes</returns>
    public byte[] ToBytes()
    {
        int totalLength = 6 + 1 + 1 + 4 + 4 + 1 + 2 + Payload.Length + 2;
        byte[] buffer = new byte[totalLength];
        int offset = 0;

        // Sync pattern (6 bytes)
        Array.Copy(SyncPattern, 0, buffer, offset, 6);
        offset += 6;

        // Slot (1 byte)
        buffer[offset++] = Slot;

        // Color code and packet type (1 byte combined)
        buffer[offset++] = (byte)(((ColorCode & 0x0F) << 4) | ((byte)Type & 0x0F));

        // Source ID (4 bytes, little-endian)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), SourceId);
        offset += 4;

        // Destination ID (4 bytes, little-endian)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset, 4), DestinationId);
        offset += 4;

        // Call type (1 byte)
        buffer[offset++] = (byte)CallType;

        // Sequence number (2 bytes, little-endian)
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset, 2), SequenceNumber);
        offset += 2;

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
    /// Validates the CRC checksum of the packet
    /// </summary>
    /// <param name="rawData">Raw packet bytes</param>
    /// <returns>True if CRC is valid</returns>
    public static bool ValidateCrc(byte[] rawData)
    {
        if (rawData.Length < 2)
        {
            return false;
        }

        // Extract stored CRC
        ushort storedCrc = BinaryPrimitives.ReadUInt16LittleEndian(rawData.AsSpan(rawData.Length - 2, 2));

        // Calculate CRC of data (excluding the CRC bytes)
        ushort calculatedCrc = CalculateCrc(rawData, 0, rawData.Length - 2);

        return storedCrc == calculatedCrc;
    }

    /// <summary>
    /// Calculate CRC-CCITT checksum (polynomial 0x1021, init 0xFFFF)
    /// </summary>
    /// <param name="data">Data to calculate CRC for</param>
    /// <param name="offset">Start offset</param>
    /// <param name="length">Length of data</param>
    /// <returns>CRC-CCITT checksum</returns>
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
}
