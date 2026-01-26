using HyteraGateway.Audio.Native.Mbelib;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Audio.Codecs.Ambe;

/// <summary>
/// AMBE+2 codec using mbelib (GPL)
/// Decodes 33-byte Hytera AMBE frames to 16-bit PCM @ 8kHz
/// </summary>
public class MbelibAmbeCodec : IAmbeCodec, IDisposable
{
    private readonly ILogger<MbelibAmbeCodec> _logger;
    private readonly MbeParamsHandle _currentParams;
    private readonly MbeParamsHandle _previousParams;
    private readonly MbeParamsHandle _previousParamsEnhanced;
    
    private const int AMBE_FRAME_SIZE = 33;      // Hytera AMBE+2 frame size
    private const int PCM_SAMPLES_PER_FRAME = 160; // 20ms @ 8kHz
    private const int SAMPLE_RATE = 8000;
    
    public MbelibAmbeCodec(ILogger<MbelibAmbeCodec> logger)
    {
        _logger = logger;
        
        // Allocate mbelib parameter structures
        _currentParams = new MbeParamsHandle();
        _previousParams = new MbeParamsHandle();
        _previousParamsEnhanced = new MbeParamsHandle();
        
        // Initialize parameters
        MbelibNative.mbe_initMbeParms(_currentParams.DangerousGetHandle(), 
                                      _previousParams.DangerousGetHandle(), 
                                      _previousParamsEnhanced.DangerousGetHandle());
        
        _logger.LogInformation("MbelibAmbeCodec initialized (GPL licensed)");
    }
    
    /// <summary>
    /// Decode AMBE+2 frame to PCM samples
    /// </summary>
    /// <param name="ambeFrame">33-byte AMBE frame from Hytera</param>
    /// <returns>320 bytes (160 samples * 2 bytes/sample) @ 8kHz 16-bit PCM</returns>
    public byte[] DecodeToPcm(byte[] ambeFrame)
    {
        if (ambeFrame == null || ambeFrame.Length != AMBE_FRAME_SIZE)
        {
            _logger.LogWarning("Invalid AMBE frame size: expected {Expected}, got {Actual}", 
                AMBE_FRAME_SIZE, ambeFrame?.Length ?? 0);
            return GenerateSilence();
        }
        
        try
        {
            // Extract 49-bit AMBE data from Hytera frame (skip header/footer)
            byte[] ambeData = ExtractAmbeData(ambeFrame);
            
            // Decode to float samples
            float[] audioFloat = new float[PCM_SAMPLES_PER_FRAME];
            int errs = 0, errs2 = 0;
            
            int result = MbelibNative.mbe_processAmbe2450Dataf(
                audioFloat,
                ref errs,
                ref errs2,
                ambeData,
                _currentParams.DangerousGetHandle(),
                _previousParams.DangerousGetHandle(),
                _previousParamsEnhanced.DangerousGetHandle(),
                uvquality: 3
            );
            
            if (errs > 0 || errs2 > 0)
            {
                _logger.LogDebug("AMBE decode errors: errs={Errs}, errs2={Errs2}", errs, errs2);
            }
            
            // Convert float to 16-bit PCM
            short[] audioShort = new short[PCM_SAMPLES_PER_FRAME];
            MbelibNative.mbe_floatToShort(audioShort, audioFloat, PCM_SAMPLES_PER_FRAME);
            
            // Convert short[] to byte[]
            byte[] pcmBytes = new byte[PCM_SAMPLES_PER_FRAME * 2];
            Buffer.BlockCopy(audioShort, 0, pcmBytes, 0, pcmBytes.Length);
            
            return pcmBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decode AMBE frame");
            return GenerateSilence();
        }
    }
    
    /// <summary>
    /// Encode PCM to AMBE+2 (not implemented - mbelib decode-only)
    /// </summary>
    public byte[] EncodeFromPcm(byte[] pcmData)
    {
        // mbelib is decode-only
        // For TX, would need DVSI hardware or licensed encoder
        throw new NotImplementedException("AMBE encoding not supported by mbelib (decode-only)");
    }
    
    private byte[] ExtractAmbeData(byte[] hyteraFrame)
    {
        // Hytera AMBE+2 frame structure (33 bytes):
        // [0-2]: Header/sync
        // [3-9]: AMBE data (49 bits = 7 bytes, but packed differently)
        // [10-32]: FEC, additional data
        
        // TODO: Reverse engineer exact Hytera AMBE frame structure
        // For now, extract middle 7 bytes as AMBE data
        byte[] ambeData = new byte[7];
        Array.Copy(hyteraFrame, 3, ambeData, 0, 7);
        return ambeData;
    }
    
    private byte[] GenerateSilence()
    {
        return new byte[PCM_SAMPLES_PER_FRAME * 2]; // All zeros = silence
    }
    
    public void Dispose()
    {
        _currentParams?.Dispose();
        _previousParams?.Dispose();
        _previousParamsEnhanced?.Dispose();
    }
}
