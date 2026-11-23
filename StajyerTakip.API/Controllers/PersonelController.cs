using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business;
using StajyerTakip.Entities;

namespace StajyerTakip.API.Controllers;

[ApiController]
[Route("api/Personel")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PersonelController(IPersonelService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonelDTO>>> Get(CancellationToken cancellationToken)
        => Ok(await service.TumunuGetirAsync(cancellationToken));
}
