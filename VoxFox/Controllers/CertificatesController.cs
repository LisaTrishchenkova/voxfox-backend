using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Certificate;
using VoxFox.Models.DTOs;
using VoxFox.Models.DTOs.Certificate;

namespace VoxFox.Controllers;

[ApiController]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [HttpGet("api/Users/certificates")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<CertificateDto>))]
    public async Task<ActionResult<IList<CertificateDto>>> GetMyCertificates()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _certificateService.GetMyCertificatesAsync(userId.Value);
        return Ok(result.Data);
    }

    [HttpGet("api/certificates/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CertificateDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateDto>> GetById([FromRoute] Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _certificateService.GetByIdAsync(id, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpGet("api/certificates/{id}/download")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPdf([FromRoute] Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _certificateService.GeneratePdfAsync(id, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return File(result.Data!, "application/pdf", $"certificate-{id}.pdf");
    }

    [HttpGet("api/certificates/verify/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CertificateDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CertificateDto>> Verify([FromRoute] string token)
    {
        var result = await _certificateService.VerifyAsync(token);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }
}
