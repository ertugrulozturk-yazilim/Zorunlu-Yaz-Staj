using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StajyerTakip.Entities;
using System;
using System.Collections.Generic;

namespace StajyerTakip.Business
{
    public class GorevService : IGorevService
    {
        private readonly string _connectionString;

        public GorevService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection bulunamadı.");
        }

        // TÜM GÖREVLERİ GETİR
        public List<GorevDTO> TumGorevleriGetir()
        {
            var liste = new List<GorevDTO>();

            using var con = new SqlConnection(_connectionString);

            using var cmd = new SqlCommand(@"
                SELECT
                    Id,
                    PersonelId,
                    Baslik,
                    Aciklama,
                    Durum
                FROM dbo.Gorevler
                WHERE IsDeleted = 0
                ORDER BY Id DESC", con);

            con.Open();

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                liste.Add(new GorevDTO
                {
                    Id = reader.GetInt32(0),
                    PersonelId = reader.GetInt32(1),
                    Baslik = reader.GetString(2),
                    Aciklama = reader.IsDBNull(3)
                        ? string.Empty
                        : reader.GetString(3),
                    Durum = reader.GetString(4)
                });
            }

            return liste;
        }

        // YENİ GÖREV EKLE
        public bool GorevEkle(GorevDTO gorev)
        {
            if (string.IsNullOrWhiteSpace(gorev.Baslik))
                return false;

            try
            {
                using var con = new SqlConnection(_connectionString);

                using var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Gorevler
                    (
                        PersonelId,
                        Baslik,
                        Aciklama,
                        Durum
                    )
                    VALUES
                    (
                        @PersonelId,
                        @Baslik,
                        @Aciklama,
                        @Durum
                    )", con);

                cmd.Parameters.AddWithValue(
                    "@PersonelId",
                    gorev.PersonelId);

                cmd.Parameters.AddWithValue(
                    "@Baslik",
                    gorev.Baslik);

                cmd.Parameters.AddWithValue(
                    "@Aciklama",
                    (object?)gorev.Aciklama ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@Durum",
                    string.IsNullOrWhiteSpace(gorev.Durum)
                        ? "Yapılacak"
                        : gorev.Durum);

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(
                    $"Veritabanı Hatası: {ex.Message}");

                return false;
            }
        }

        // GÖREV DURUMUNU GÜNCELLE
        public bool DurumGuncelle(int id, string yeniDurum)
        {
            if (string.IsNullOrWhiteSpace(yeniDurum))
                return false;

            try
            {
                using var con = new SqlConnection(_connectionString);

                using var cmd = new SqlCommand(@"
                    UPDATE dbo.Gorevler
                    SET Durum = @Durum
                    WHERE Id = @Id
                      AND IsDeleted = 0", con);

                cmd.Parameters.AddWithValue(
                    "@Durum",
                    yeniDurum);

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(
                    $"Güncelleme Hatası: {ex.Message}");

                return false;
            }
        }

        // GÖREV SİL
        // Fiziksel olarak silmez, IsDeleted = 1 yapar.
        public bool GorevSil(int id)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                using var cmd = new SqlCommand(@"
                    UPDATE dbo.Gorevler
                    SET IsDeleted = 1
                    WHERE Id = @Id
                      AND IsDeleted = 0", con);

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(
                    $"Silme Hatası: {ex.Message}");

                return false;
            }
        }
    }
}