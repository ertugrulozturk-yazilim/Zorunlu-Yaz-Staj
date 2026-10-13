using StajyerTakip.Entities;
using System.Collections.Generic;

namespace StajyerTakip.Business
{
    public interface IGorevService
    {
        List<GorevDTO> TumGorevleriGetir();

        bool GorevEkle(GorevDTO gorev);

        bool DurumGuncelle(int id, string yeniDurum);

        bool GorevSil(int id);
    }
}