using System.Runtime.InteropServices;

namespace HyteraGateway.Audio.Native.Mbelib;

/// <summary>
/// P/Invoke wrapper for mbelib AMBE decoder
/// License: GPL (mbelib is GPL licensed)
/// </summary>
internal static class MbelibNative
{
    private const string LibraryName = "mbe";
    
    // Initialize decoder state
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr mbe_initMbeParms(IntPtr cur_mp, IntPtr prev_mp, IntPtr prev_mp_enhanced);
    
    // Decode AMBE frame to PCM samples
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int mbe_processAmbe2450Dataf(
        [Out] float[] aout_buf,         // Output: 160 float samples
        ref int errs,                    // Output: error count
        ref int errs2,                   // Output: error count 2
        [In] byte[] ambe_fr,            // Input: 49-bit AMBE frame (7 bytes)
        IntPtr cur_mp,                   // Current frame parameters
        IntPtr prev_mp,                  // Previous frame parameters
        IntPtr prev_mp_enhanced,         // Previous enhanced parameters
        int uvquality                    // UV quality flag
    );
    
    // Convert float samples to 16-bit PCM
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void mbe_floatToShort(
        [Out] short[] audio_out,        // Output: 160 int16 samples
        [In] float[] audio_in,          // Input: 160 float samples
        int length                       // Number of samples (160)
    );
}

/// <summary>
/// Safe handle for mbelib parameter structures
/// </summary>
internal class MbeParamsHandle : SafeHandle
{
    private const int MBE_PARMS_SIZE = 256; // Adjust based on mbelib struct size
    
    public MbeParamsHandle() : base(IntPtr.Zero, true)
    {
        SetHandle(Marshal.AllocHGlobal(MBE_PARMS_SIZE));
    }
    
    public override bool IsInvalid => handle == IntPtr.Zero;
    
    protected override bool ReleaseHandle()
    {
        if (handle != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(handle);
            handle = IntPtr.Zero;
        }
        return true;
    }
}
