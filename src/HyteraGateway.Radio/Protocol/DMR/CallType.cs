namespace HyteraGateway.Radio.Protocol.DMR;

/// <summary>
/// DMR call types based on ETSI TS 102 361
/// </summary>
public enum CallType
{
    /// <summary>
    /// Private call to a specific DMR ID
    /// </summary>
    Private = 0,

    /// <summary>
    /// Group call to a talkgroup
    /// </summary>
    Group = 1,

    /// <summary>
    /// All call (broadcast to all radios)
    /// </summary>
    All = 2
}
