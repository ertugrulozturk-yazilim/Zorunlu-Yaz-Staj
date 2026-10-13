using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization.Metadata;
using StajyerTakip.Business; // 1. BU SATIR ŞART: Servisi bulabilmesi için ekledik.

var builder = WebApplication.CreateBuilder(args);

// API servislerini ekle ve JSON hatasını kökten çözen ayar
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // .NET'in varsayılan çözücüsünü atayarak 'read-only' hatasını engelliyoruz
    options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
});

// 2. İŞTE EKSİK OLAN VE HATAYI ÇÖZEN HAYAT KURTARICI SATIR
builder.Services.AddScoped<IGorevService, GorevService>();

// CORS Politikası: Frontend uygulamanın API'ye erişebilmesi için gerekli
builder.Services.AddCors(options =>
{
    options.AddPolicy("IzinVer", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// CORS politikasını etkinleştir
app.UseCors("IzinVer");

// Controller rotalarını haritala
app.MapControllers();

app.Run();