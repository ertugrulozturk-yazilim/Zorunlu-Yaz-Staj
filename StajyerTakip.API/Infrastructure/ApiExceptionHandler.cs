using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StajyerTakip.Business;
using StajyerTakip.DataAccess;

namespace StajyerTakip.API.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
            return true;
        }
        var (status, title, detail) = exception switch
        {
            VeritabaniYapilandirmaHatasi e => (503, "Veritabanı bağlantısı ayarlanmamış", e.Message),
            IslemHatasi { Tur: IslemHataTuru.GecersizVeri } e => (400, "Geçersiz veri", e.Message),
            IslemHatasi { Tur: IslemHataTuru.Bulunamadi } e => (404, "Görev bulunamadı", e.Message),
            IslemHatasi { Tur: IslemHataTuru.SurumUyusmazligi } e => (412, "Kayıt değişmiş", e.Message),
            SqlException { Number: 18456 } => (503, "Veritabanına giriş yapılamadı", "Azure SQL kullanıcı adı ve parolasını kontrol edin. ILK_ONCE_OKU.md dosyasındaki bağlantı ayarlarına bakın."),
            SqlException { Number: 40615 } => (503, "Veritabanına erişim izni yok", "API'nin çalıştığı bilgisayarın güncel IP adresini Azure SQL güvenlik duvarında kontrol edin."),
            SqlException => (503, "Veritabanına erişilemiyor", "Azure SQL bağlantısını ve veritabanının hazır olduğunu kontrol edin. Ayrıntı API günlüğünde bulunur."),
            _ => (500, "İşlem tamamlanamadı", "Beklenmeyen bir sunucu hatası oluştu. Ayrıntı API günlüğünde bulunur.")
        };
        if (status >= 500) logger.LogError(exception, "API hatası. TraceId: {TraceId}", context.TraceIdentifier);
        else logger.LogInformation("İstek reddedildi ({StatusCode}): {Reason}", status, detail);
        context.Response.StatusCode = status;
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = context.Request.Path };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
