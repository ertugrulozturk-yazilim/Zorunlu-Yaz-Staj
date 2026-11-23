/* SSMS'te HEDEF VERITABANINI seçerek çalıştırın.
   Veritabanı veya mevcut tablolar silinmez; mevcut kayıtlar güncellenmez.
   Paylaşılan eski schema.sql bu alanları zaten içerdiğinden mevcut Azure veritabanında
   genellikle değişiklik gerekmez. Betik boş bir veritabanını da hazırlayabilir. */
SET XACT_ABORT ON;
GO
IF OBJECT_ID(N'dbo.Departmanlar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departmanlar
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departmanlar PRIMARY KEY,
        DepartmanAdi NVARCHAR(100) NOT NULL
    );
END;
GO
IF OBJECT_ID(N'dbo.Pozisyonlar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pozisyonlar
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pozisyonlar PRIMARY KEY,
        DepartmanId INT NOT NULL,
        PozisyonAdi NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_Pozisyonlar_Departmanlar FOREIGN KEY (DepartmanId) REFERENCES dbo.Departmanlar(Id)
    );
END;
GO
IF OBJECT_ID(N'dbo.Personeller', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Personeller
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Personeller PRIMARY KEY,
        PozisyonId INT NOT NULL,
        Ad NVARCHAR(50) NOT NULL,
        Soyad NVARCHAR(50) NOT NULL,
        Email NVARCHAR(150) NULL,
        CONSTRAINT FK_Personeller_Pozisyonlar FOREIGN KEY (PozisyonId) REFERENCES dbo.Pozisyonlar(Id)
    );
END;
GO
IF OBJECT_ID(N'dbo.Gorevler', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gorevler
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Gorevler PRIMARY KEY,
        PersonelId INT NOT NULL,
        Baslik NVARCHAR(150) NOT NULL,
        Aciklama NVARCHAR(2000) NULL,
        Durum NVARCHAR(20) NOT NULL,
        OlusturmaTarihi DATETIME2(7) NOT NULL CONSTRAINT DF_Gorevler_OlusturmaTarihi DEFAULT SYSUTCDATETIME(),
        BitisTarihi DATETIME2(7) NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_Gorevler_IsDeleted DEFAULT 0,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_Gorevler_Personeller FOREIGN KEY (PersonelId) REFERENCES dbo.Personeller(Id),
        CONSTRAINT CHK_Gorevler_Durum CHECK (Durum IN (N'Yapılacak', N'Devam Ediyor', N'Tamamlandı'))
    );
END;
GO
IF COL_LENGTH(N'dbo.Gorevler', N'IsDeleted') IS NULL
    ALTER TABLE dbo.Gorevler ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Gorevler_IsDeleted_Sunum DEFAULT 0 WITH VALUES;
GO
IF COL_LENGTH(N'dbo.Gorevler', N'RowVersion') IS NULL
    ALTER TABLE dbo.Gorevler ADD RowVersion ROWVERSION NOT NULL;
GO
IF COL_LENGTH(N'dbo.Gorevler', N'BitisTarihi') IS NULL
    ALTER TABLE dbo.Gorevler ADD BitisTarihi DATETIME2(7) NULL;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Gorevler') AND name = N'Aciklama' AND max_length > 0 AND max_length < 4000)
    ALTER TABLE dbo.Gorevler ALTER COLUMN Aciklama NVARCHAR(2000) NULL;
GO
IF COL_LENGTH(N'dbo.Gorevler', N'OlusturmaTarihi') IS NULL
    ALTER TABLE dbo.Gorevler ADD OlusturmaTarihi DATETIME2(7) NOT NULL CONSTRAINT DF_Gorevler_OlusturmaTarihi_Sunum DEFAULT SYSUTCDATETIME() WITH VALUES;
GO
-- Kısıt zaten varsa yeniden oluşturulmaz. Mevcut geçersiz durumlar varsa betik
-- onları sessizce değiştirmek yerine doğrulama hatası verir.
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Gorevler') AND name = N'CHK_Gorevler_Durum')
    ALTER TABLE dbo.Gorevler WITH CHECK ADD CONSTRAINT CHK_Gorevler_Durum CHECK (Durum IN (N'Yapılacak', N'Devam Ediyor', N'Tamamlandı'));
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Gorevler') AND name = N'RowVersion' AND system_type_id <> 189)
    THROW 50001, 'RowVersion sütunu ROWVERSION/TIMESTAMP türünde olmalıdır. Mevcut sütunu veya kayıtları otomatik olarak değiştirmedim.', 1;
GO
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME IN ('Departmanlar','Pozisyonlar','Personeller','Gorevler')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
