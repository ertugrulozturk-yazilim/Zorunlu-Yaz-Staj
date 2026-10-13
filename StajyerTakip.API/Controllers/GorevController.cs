using Microsoft.AspNetCore.Mvc;
using StajyerTakip.Business;
using StajyerTakip.Entities;

namespace StajyerTakip.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GorevController : ControllerBase
    {
        private readonly IGorevService _gorevService;

        public GorevController(IGorevService gorevService)
        {
            _gorevService = gorevService;
        }

        // TÜM GÖREVLERİ GETİR
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_gorevService.TumGorevleriGetir());
        }

        // YENİ GÖREV EKLE
        [HttpPost]
        public IActionResult Post([FromBody] GorevDTO yeniGorev)
        {
            var basariliMi = _gorevService.GorevEkle(yeniGorev);

            if (!basariliMi)
                return BadRequest("Geçersiz veri girdiniz.");

            return StatusCode(201, "Görev başarıyla eklendi.");
        }

        // GÖREV DURUMUNU GÜNCELLE
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string yeniDurum)
        {
            var basariliMi = _gorevService.DurumGuncelle(id, yeniDurum);

            if (!basariliMi)
                return NotFound("Görev bulunamadı.");

            return Ok("Durum güncellendi.");
        }

        // GÖREV SİL
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var basariliMi = _gorevService.GorevSil(id);

            if (!basariliMi)
                return NotFound("Görev bulunamadı.");

            return Ok("Görev başarıyla silindi.");
        }
    }
}