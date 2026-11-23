using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business;
using StajyerTakip.Entities;

namespace StajyerTakip.API.Controllers;

[ApiController]
[Route("api/Gorev")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class GorevController(IGorevService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GorevDTO>>> Get(CancellationToken cancellationToken)
        => Ok(await service.TumunuGetirAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GorevDTO>> GetById([Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var gorev = await service.GetirAsync(id, cancellationToken);
        Response.Headers.ETag = $"\"{gorev.RowVersion}\"";
        return Ok(gorev);
    }

    [HttpPost]
    public async Task<ActionResult<GorevDTO>> Post(GorevOlusturDTO dto, CancellationToken cancellationToken)
    {
        var gorev = await service.EkleAsync(dto, cancellationToken);
        Response.Headers.ETag = $"\"{gorev.RowVersion}\"";
        return CreatedAtAction(nameof(GetById), new { id = gorev.Id }, gorev);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GorevDTO>> Put([Range(1, int.MaxValue)] int id, GorevDurumGuncelleDTO dto, CancellationToken cancellationToken)
    {
        var version = SurumuOku();
        if (version is null) return SurumHatasi();
        var gorev = await service.DurumGuncelleAsync(id, dto.Durum!.Value, version, cancellationToken);
        Response.Headers.ETag = $"\"{gorev.RowVersion}\"";
        return Ok(gorev);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id, CancellationToken cancellationToken)
    {
        var version = SurumuOku();
        if (version is null) return SurumHatasi();
        await service.SilAsync(id, version, cancellationToken);
        return NoContent();
    }

    private byte[]? SurumuOku()
    {
        var value = Request.Headers.IfMatch.ToString();
        if (value.Length < 2 || value[0] != '"' || value[^1] != '"') return null;
        try
        {
            var bytes = Convert.FromBase64String(value[1..^1]);
            return bytes.Length == 8 ? bytes : null;
        }
        catch (FormatException) { return null; }
    }

    private ObjectResult SurumHatasi()
    {
        var missing = string.IsNullOrWhiteSpace(Request.Headers.IfMatch.ToString());
        return Problem(statusCode: missing ? 428 : 400, title: missing ? "Kayıt sürümü gerekli" : "Geçersiz kayıt sürümü",
            detail: "Önce görevi GET ile alın. RowVersion değerini If-Match başlığında çift tırnak içinde gönderin.");
    }
}
