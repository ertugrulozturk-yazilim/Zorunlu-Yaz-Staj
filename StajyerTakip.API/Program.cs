using StajyerTakip.API.Infrastructure;
using StajyerTakip.API.Serialization;
using StajyerTakip.Business;
using StajyerTakip.DataAccess;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new GorevDurumuJsonConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton(sp => new SqlConnectionFactory(
    sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection") ?? ""));
builder.Services.AddScoped<IGorevRepository, SqlGorevRepository>();
builder.Services.AddScoped<IPersonelRepository, SqlPersonelRepository>();
builder.Services.AddScoped<IGorevService, GorevService>();
builder.Services.AddScoped<IPersonelService, PersonelService>();

var app = builder.Build();
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next(context);
});
// Arayüz ve API aynı origin üzerinde: ayrı Live Server veya CORS ayarı gerekmez.
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok(new { status = "API çalışıyor" }));
app.MapControllers();
app.Run();

public partial class Program { }
