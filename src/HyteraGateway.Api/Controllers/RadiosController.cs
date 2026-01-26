using HyteraGateway.Core.Interfaces;
using HyteraGateway.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HyteraGateway.Api.Controllers;

/// <summary>
/// Controller for managing Hytera radios
/// </summary>
[ApiController]
[Route("api/radios")]
public class RadiosController : ControllerBase
{
    private readonly IRadioService _radioService;
    private readonly ILogger<RadiosController> _logger;

    /// <summary>
    /// Initializes a new instance of the RadiosController
    /// </summary>
    /// <param name="radioService">Radio service instance</param>
    /// <param name="logger">Logger instance</param>
    public RadiosController(IRadioService radioService, ILogger<RadiosController> logger)
    {
        _radioService = radioService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the status of a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Radio status</returns>
    [HttpGet("{dmrId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(int dmrId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting status for radio {DmrId}", dmrId);

        var status = await _radioService.GetStatusAsync(dmrId, cancellationToken);

        if (status == null)
        {
            return NotFound(new { message = $"Radio {dmrId} not found" });
        }

        return Ok(status);
    }

    /// <summary>
    /// Sends PTT command to a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    /// <param name="request">PTT request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of PTT command</returns>
    [HttpPost("{dmrId}/ptt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendPtt(int dmrId, [FromBody] PttRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending PTT command to radio {DmrId}: {Action}", dmrId, request.Press ? "PRESS" : "RELEASE");

        var result = await _radioService.SendPttAsync(dmrId, request.Press, cancellationToken);

        if (!result)
        {
            return BadRequest(new { message = "Failed to send PTT command" });
        }

        return Ok(new { message = $"PTT {(request.Press ? "pressed" : "released")} successfully" });
    }

    /// <summary>
    /// Requests GPS position from a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>GPS position</returns>
    [HttpPost("{dmrId}/gps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestGps(int dmrId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requesting GPS position from radio {DmrId}", dmrId);

        var position = await _radioService.RequestGpsAsync(dmrId, cancellationToken);

        if (position == null)
        {
            return NotFound(new { message = "GPS position not available" });
        }

        return Ok(position);
    }

    /// <summary>
    /// Sends a text message to a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    /// <param name="request">SMS request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of SMS send</returns>
    [HttpPost("{dmrId}/sms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSms(int dmrId, [FromBody] SmsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending SMS to radio {DmrId}: {Message}", dmrId, request.Message);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message cannot be empty" });
        }

        var result = await _radioService.SendTextMessageAsync(dmrId, request.Message, cancellationToken);

        if (!result)
        {
            return BadRequest(new { message = "Failed to send SMS" });
        }

        return Ok(new { message = "SMS sent successfully" });
    }
}
