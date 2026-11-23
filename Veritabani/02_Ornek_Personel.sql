/* İSTEĞE BAĞLI: Yalnızca boş personel tablosuna örnek kayıt ekler.
   Mevcut personeller veya görevler silinmez/değiştirilmez.
   İlk olarak 01_Kurulum_ve_Guncelleme.sql çalıştırılmış olmalıdır. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.Personeller)
BEGIN
    DECLARE @DepartmanId INT, @PozisyonId INT;
    SELECT TOP (1) @DepartmanId = Id FROM dbo.Departmanlar WHERE DepartmanAdi = N'Yazılım' ORDER BY Id;
    IF @DepartmanId IS NULL
    BEGIN
        INSERT INTO dbo.Departmanlar(DepartmanAdi) VALUES (N'Yazılım');
        SET @DepartmanId = CONVERT(INT, SCOPE_IDENTITY());
    END;
    SELECT TOP (1) @PozisyonId = Id FROM dbo.Pozisyonlar WHERE DepartmanId = @DepartmanId AND PozisyonAdi = N'Yazılım Stajyeri' ORDER BY Id;
    IF @PozisyonId IS NULL
    BEGIN
        INSERT INTO dbo.Pozisyonlar(DepartmanId, PozisyonAdi) VALUES (@DepartmanId, N'Yazılım Stajyeri');
        SET @PozisyonId = CONVERT(INT, SCOPE_IDENTITY());
    END;
    INSERT INTO dbo.Personeller(PozisyonId, Ad, Soyad) VALUES (@PozisyonId, N'Demo', N'Personel');
END;
COMMIT TRANSACTION;
SELECT Id, Ad, Soyad FROM dbo.Personeller ORDER BY Id;
