namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents the type of DMR call
/// </summary>
public enum CallType
{
    /// <summary>
    /// Group call (talkgroup)
    /// </summary>
    Group,

    /// <summary>
    /// Private call (individual)
    /// </summary>
    Private,

    /// <summary>
    /// Emergency call
    /// </summary>
    Emergency,

    /// <summary>
    /// All call (broadcast)
    /// </summary>
    AllCall
}
