using System.Data;
using Microsoft.Data.SqlClient;
using StajyerTakip.Entities;

namespace StajyerTakip.DataAccess;

public sealed class SqlGorevRepository(SqlConnectionFactory connectionFactory) : IGorevRepository
{
    private const string Alanlar = "Id, PersonelId, Baslik, Aciklama, Durum, OlusturmaTarihi, BitisTarihi, RowVersion";
    private const string CiktiAlanlari = "INSERTED.Id, INSERTED.PersonelId, INSERTED.Baslik, INSERTED.Aciklama, INSERTED.Durum, INSERTED.OlusturmaTarihi, INSERTED.BitisTarihi, INSERTED.RowVersion";

    public async Task<IReadOnlyList<Gorev>> TumunuGetirAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand($"SELECT {Alanlar} FROM dbo.Gorevler WHERE IsDeleted = 0 ORDER BY Id DESC;", connection);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<Gorev>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Oku(reader));
        return result;
    }

    public async Task<Gorev?> GetirAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand($"SELECT {Alanlar} FROM dbo.Gorevler WHERE Id = @Id AND IsDeleted = 0;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Oku(reader) : null;
    }

    public async Task<Gorev> EkleAsync(Gorev gorev, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand($"""
            INSERT INTO dbo.Gorevler (PersonelId, Baslik, Aciklama, Durum, OlusturmaTarihi, IsDeleted)
            OUTPUT {CiktiAlanlari}
            VALUES (@PersonelId, @Baslik, @Aciklama, @Durum, SYSUTCDATETIME(), 0);
            """, connection);
        command.Parameters.Add("@PersonelId", SqlDbType.Int).Value = gorev.PersonelId;
        command.Parameters.Add("@Baslik", SqlDbType.NVarChar, 150).Value = gorev.Baslik;
        command.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 2000).Value = (object?)gorev.Aciklama ?? DBNull.Value;
        command.Parameters.Add("@Durum", SqlDbType.NVarChar, 20).Value = gorev.Durum.MetneCevir();
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException("Eklenen görev okunamadı.");
        return Oku(reader);
    }

    public async Task<Gorev?> DurumGuncelleAsync(int id, GorevDurumu durum, byte[] rowVersion, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand($"""
            UPDATE dbo.Gorevler
            SET Durum = @Durum,
                BitisTarihi = CASE WHEN @Durum = N'Tamamlandı' THEN COALESCE(BitisTarihi, SYSUTCDATETIME()) ELSE NULL END
            OUTPUT {CiktiAlanlari}
            WHERE Id = @Id AND IsDeleted = 0 AND RowVersion = @RowVersion;
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@Durum", SqlDbType.NVarChar, 20).Value = durum.MetneCevir();
        command.Parameters.Add("@RowVersion", SqlDbType.Binary, 8).Value = rowVersion;
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Oku(reader) : null;
    }

    public async Task<bool> SilAsync(int id, byte[] rowVersion, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand("""
            UPDATE dbo.Gorevler SET IsDeleted = 1
            WHERE Id = @Id AND IsDeleted = 0 AND RowVersion = @RowVersion;
            """, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        command.Parameters.Add("@RowVersion", SqlDbType.Binary, 8).Value = rowVersion;
        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    private static Gorev Oku(SqlDataReader reader)
    {
        if (!GorevDurumuMetinleri.TryParse(reader.GetString(4), out var durum))
            throw new InvalidOperationException("Veritabanında tanımsız bir görev durumu var.");

        return new Gorev
        {
            Id = reader.GetInt32(0), PersonelId = reader.GetInt32(1), Baslik = reader.GetString(2),
            Aciklama = reader.IsDBNull(3) ? null : reader.GetString(3), Durum = durum,
            OlusturmaTarihi = reader.GetDateTime(5),
            BitisTarihi = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
            RowVersion = reader.GetFieldValue<byte[]>(7)
        };
    }
}
