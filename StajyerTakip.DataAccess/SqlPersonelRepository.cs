using System.Data;
using Microsoft.Data.SqlClient;
using StajyerTakip.Entities;

namespace StajyerTakip.DataAccess;

public sealed class SqlPersonelRepository(SqlConnectionFactory connectionFactory) : IPersonelRepository
{
    public async Task<IReadOnlyList<PersonelDTO>> TumunuGetirAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand("""
            SELECT p.Id, p.Ad + N' ' + p.Soyad AS AdSoyad, po.PozisyonAdi, d.DepartmanAdi
            FROM dbo.Personeller AS p
            INNER JOIN dbo.Pozisyonlar AS po ON po.Id = p.PozisyonId
            INNER JOIN dbo.Departmanlar AS d ON d.Id = po.DepartmanId
            ORDER BY p.Ad, p.Soyad, p.Id;
            """, connection);
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<PersonelDTO>();
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new PersonelDTO(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
        return result;
    }

    public async Task<bool> VarMiAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.Olustur();
        await using var command = new SqlCommand("SELECT COUNT(1) FROM dbo.Personeller WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }
}
