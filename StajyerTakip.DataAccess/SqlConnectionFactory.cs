using Microsoft.Data.SqlClient;

namespace StajyerTakip.DataAccess;

public sealed class VeritabaniYapilandirmaHatasi(string message) : Exception(message);

public sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Olustur()
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new VeritabaniYapilandirmaHatasi("DefaultConnection yapılandırılmamış. ILK_ONCE_OKU.md dosyasındaki User Secrets adımını tamamlayın.");

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = false
        };
        return new SqlConnection(builder.ConnectionString);
    }
}
