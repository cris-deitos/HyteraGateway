using System.Buffers.Binary;

namespace HyteraGateway.Radio.Protocol.Hytera;

/// <summary>
/// Validates Hytera IPSC packets with detailed error reporting
/// </summary>
public static class HyteraPacketValidator
{
    /// <summary>
    /// Validates a packet and returns detailed validation result
    /// </summary>
    /// <param name="data">Raw packet bytes</param>
    /// <returns>Validation result with severity and error details</returns>
    public static ValidationResult Validate(byte[] data)
    {
        if (data == null)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = "Packet data is null"
            };
        }

        if (data.Length < 8)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = $"Packet too short: {data.Length} bytes (minimum 8 bytes required)"
            };
        }

        // Validate "PH" signature (0x50, 0x48)
        if (data[0] != 0x50 || data[1] != 0x48)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = $"Invalid signature: 0x{data[0]:X2}{data[1]:X2} (expected 0x5048 'PH')"
            };
        }

        // Check length field
        ushort declaredLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(6, 2));
        
        // Validate length bounds (19-65535 bytes)
        if (declaredLength < 19)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = $"Declared length too small: {declaredLength} bytes (minimum 19 bytes)"
            };
        }

        if (declaredLength > 65535)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = $"Declared length exceeds maximum: {declaredLength} bytes (maximum 65535 bytes)"
            };
        }

        // Verify length matches actual packet length
        if (declaredLength != data.Length)
        {
            return new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                ErrorMessage = $"Length mismatch: declared {declaredLength} bytes, actual {data.Length} bytes"
            };
        }

        // Validate CRC if packet is long enough
        if (data.Length >= 19)
        {
            ushort declaredCrc = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(data.Length - 2, 2));
            ushort calculatedCrc = HyteraIPSCPacket.CalculateCrc(data, 0, data.Length - 2);
            
            if (declaredCrc != calculatedCrc)
            {
                return new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    ErrorMessage = $"CRC mismatch: declared 0x{declaredCrc:X4}, calculated 0x{calculatedCrc:X4}"
                };
            }
        }

        // Check if command code is known
        if (data.Length >= 10)
        {
            ushort commandCode = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(8, 2));
            
            if (!Enum.IsDefined(typeof(HyteraCommand), commandCode))
            {
                return new ValidationResult
                {
                    Severity = ValidationSeverity.Warning,
                    ErrorMessage = $"Unknown command code: 0x{commandCode:X4}"
                };
            }
        }

        // All validations passed
        return new ValidationResult
        {
            Severity = ValidationSeverity.None,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Quick validation check (returns true/false only)
    /// </summary>
    /// <param name="data">Raw packet bytes</param>
    /// <returns>True if packet is valid (no errors)</returns>
    public static bool IsValid(byte[] data)
    {
        var result = Validate(data);
        return result.Severity != ValidationSeverity.Error;
    }
}

/// <summary>
/// Validation result with severity level
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Severity of validation result
    /// </summary>
    public ValidationSeverity Severity { get; set; }

    /// <summary>
    /// Error or warning message (null if no issues)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// True if validation passed without errors (warnings are allowed)
    /// </summary>
    public bool IsValid => Severity != ValidationSeverity.Error;

    /// <summary>
    /// True if validation passed without any issues
    /// </summary>
    public bool IsPerfect => Severity == ValidationSeverity.None;
}

/// <summary>
/// Validation severity levels
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// No issues detected
    /// </summary>
    None = 0,

    /// <summary>
    /// Warning - packet is valid but has non-critical issues
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error - packet is invalid and should be rejected
    /// </summary>
    Error = 2
}
