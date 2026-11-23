namespace StajyerTakip.Business;

public enum IslemHataTuru { GecersizVeri, Bulunamadi, SurumUyusmazligi }

public sealed class IslemHatasi(IslemHataTuru tur, string message) : Exception(message)
{
    public IslemHataTuru Tur { get; } = tur;
}
