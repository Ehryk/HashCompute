
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

USE master;
IF EXISTS(SELECT name FROM master.sys.server_principals WHERE name = 'HashSearch')
	DROP LOGIN HashSearch;
IF EXISTS(SELECT * FROM sys.database_principals WHERE name = N'HashSearch')
	DROP USER HashSearch;
IF EXISTS(select * from sys.databases where name='HashSearch')
	DROP DATABASE HashSearch;

GO

CREATE DATABASE HashSearch
GO

USE HashSearch
GO

CREATE LOGIN HashSearch WITH PASSWORD = 'password', DEFAULT_DATABASE = HashSearch;
GO

CREATE USER HashSearch FOR LOGIN HashSearch;
GO

/* Algorithm Table */

CREATE TABLE [dbo].[Algorithm]
(
	ID int PRIMARY KEY IDENTITY(1,1),

	Name nvarchar(100) not null,
	[Length] int null,
	TypeName nvarchar(100) null,
	[Description] nvarchar(900) null,

	Polynomial_Normal varbinary(255) null,
	Polynomial_Reversed varbinary(255) null,

	InsertUser nvarchar(128) not null default(suser_sname()),
	InsertDate datetime2 not null default(sysdatetime()),
	UpdateUser nvarchar(128) null,
	UpdateDate datetime2 null
)

GO

CREATE TRIGGER [dbo].[trg_Algorithm_Audit] ON [dbo].[Algorithm]
FOR INSERT, UPDATE
AS
BEGIN

	IF NOT EXISTS (SELECT 1 FROM deleted)
	BEGIN
		-- Insert
		update	[Algorithm]
		set		InsertUser = suser_sname(),
				InsertDate = sysdatetime()
		from	[Algorithm]
		join	Inserted on [Algorithm].ID = Inserted.ID
	END
	ELSE
	BEGIN
		-- Update
		update	[Algorithm]
		set		UpdateUser = suser_sname(),
				UpdateDate = sysdatetime()
		from	[Algorithm]
		join	Inserted on [Algorithm].ID = Inserted.ID
	END

END

GO

/* Search Table */

CREATE TABLE [dbo].[Search]
(
	ID int PRIMARY KEY IDENTITY(1,1),
	AlgorithmID int not null,

	MachineName nvarchar(100) null,
	InputCount bigint not null default 0,
	Seed varbinary(4096) null,
	SearchMode nvarchar(100) null,
	LastInput varbinary(4096) null,
	Completed bit not null default 0,
	
	StartTime datetime2 not null default(sysdatetime()),
	EndTime datetime2 null,
	SearchSeconds int null,

	InsertUser nvarchar(128) not null default(suser_sname()),
	InsertDate datetime2 not null default(sysdatetime()),
	UpdateUser nvarchar(128) null,
	UpdateDate datetime2 null
)

GO

ALTER TABLE [dbo].[Search] ADD CONSTRAINT FK_Search_Algorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[Algorithm](ID)

GO

CREATE TRIGGER [dbo].[trg_Search_Audit] ON [dbo].[Search]
FOR INSERT, UPDATE
AS
BEGIN

	IF NOT EXISTS (SELECT 1 FROM deleted)
	BEGIN
		-- Insert
		update	Search
		set		InsertUser = suser_sname(),
				InsertDate = sysdatetime()
		from	Search
		join	Inserted on Search.ID = Inserted.ID
	END
	ELSE
	BEGIN
		-- Update
		update	Search
		set		UpdateUser = suser_sname(),
				UpdateDate = sysdatetime()
		from	Search
		join	Inserted on Search.ID = Inserted.ID
	END

END

GO

/* Similarity Table */

CREATE TABLE [dbo].[Similarity]
(
	ID int PRIMARY KEY IDENTITY(1,1),

	AlgorithmID int not null,
	Input varbinary(4096) not null,
	Result varbinary(4096) not null,
	
	BitSimilarity int null,
	ByteSimilarity int null,
	FixPoint bit not null default 0,

	InsertDate datetime2 not null default(sysdatetime())
)

GO

ALTER TABLE [dbo].[Similarity] ADD CONSTRAINT FK_Similarity_Algorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[Algorithm](ID)

GO

/* Create AlgorithmView */

CREATE VIEW [dbo].[AlgorithmView]
AS
WITH Similarities (AlgorithmID, SimilarityCount, MaxBitSimilarity, MaxByteSimilarity, FixPointFound) 
AS
(
	SELECT 
		AlgorithmID, 
		count(*) as SimilarityCount, 
		max(BitSimilarity) as MaxBitSimilarity, 
		max(ByteSimilarity) as MaxByteSimilarity,
		max(CAST(FixPoint AS tinyint)) as FixPointFound
	FROM
		Similarity s
	GROUP BY AlgorithmID
), Searching (AlgorithmID, SearchCount, InputCount, TotalSeconds) 
AS
(
	SELECT 
		AlgorithmID, 
		count(*) as SearchCount, 
		sum(InputCount) as InputCount, 
		sum(SearchSeconds) as TotalSeconds
	FROM
		Search s
	GROUP BY AlgorithmID
)
SELECT
	a.*,
	si.SimilarityCount, 
	si.MaxBitSimilarity,
	si.MaxByteSimilarity,
	si.FixPointFound,
	se.InputCount,
	se.SearchCount,
	se.TotalSeconds
FROM 
	[Algorithm] a LEFT OUTER JOIN
	Similarities si on si.AlgorithmID = a.ID LEFT OUTER JOIN
	Searching se on se.AlgorithmID = a.ID

GO

/* Create SearchView */

CREATE VIEW [dbo].[SearchView]
AS
SELECT
	s.*,
	1.0 * s.InputCount / s.SearchSeconds as 'Rate',
	a.Name as 'AlgorithmName',
	a.[Length],
	a.TypeName
FROM 
	Search s LEFT OUTER JOIN
	[Algorithm] a on s.AlgorithmID = a.ID

GO

/* Create SimilarityView */

CREATE VIEW [dbo].[SimilarityView]
AS
SELECT
	s.*,
	CASE 
		WHEN BitSimilarity = [Length] THEN 1
		ELSE 0
		END as 'FullBitSimilarity',
	a.Name as 'AlgorithmName',
	a.[Length],
	a.TypeName
FROM 
	Similarity s LEFT OUTER JOIN
	[Algorithm] a on s.AlgorithmID = a.ID

GO

/* Create Procedures */

CREATE PROCEDURE [dbo].[Algorithm_Insert] 
(
	@Name nvarchar(100),
	@Length int = null,
	@TypeName nvarchar(100) = null
)
AS
BEGIN
	BEGIN TRY
		DECLARE @Inserted TABLE (ID int)
		
		INSERT INTO [Algorithm] (Name, [Length], TypeName)
		OUTPUT inserted.ID INTO @Inserted
		VALUES (@Name, @Length, @TypeName)

		SELECT * FROM [Algorithm] a INNER JOIN @Inserted i on a.ID = i.ID
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH
END

GO

CREATE PROCEDURE [dbo].[Similarity_Insert] 
(
	@AlgorithmName nvarchar(100),
	@Input varbinary(4096),
	@Result varbinary(4096),
	@BitSimilarity int = null,
	@ByteSimilarity int = null,
	@FixPoint bit = null
)
AS
BEGIN
	BEGIN TRY
		DECLARE @AlgorithmID int
		SELECT @AlgorithmID = ID FROM [Algorithm] WHERE Name = @AlgorithmName

		IF (@FixPoint is null)
		BEGIN
			SET @FixPoint = 0
			IF (@Input = @Result)
				SET @FixPoint = 1
		END

		DECLARE @Inserted TABLE (ID int)
		
		INSERT INTO Similarity(AlgorithmID, Input, Result, BitSimilarity, ByteSimilarity, FixPoint)
		OUTPUT inserted.ID INTO @Inserted
		VALUES (@AlgorithmID, @Input, @Result, @BitSimilarity, @ByteSimilarity, @FixPoint)

		SELECT * FROM Similarity s INNER JOIN @Inserted i on s.ID = i.ID
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH
END

GO

CREATE PROCEDURE [dbo].[Search_Start] 
(
	@AlgorithmName nvarchar(100),
	@MachineName nvarchar(100) = null,
	@SearchMode nvarchar(100) = null,
	@Seed varbinary(4096) = null
)
AS
BEGIN
	BEGIN TRY
		DECLARE @AlgorithmID int
		SELECT @AlgorithmID = ID FROM [Algorithm] WHERE Name = @AlgorithmName

		DECLARE @Inserted TABLE (ID int)
		
		INSERT INTO Search(AlgorithmID, MachineName, SearchMode, Seed, StartTime)
		OUTPUT inserted.ID INTO @Inserted
		VALUES (@AlgorithmID, @MachineName, @SearchMode, @Seed, SYSDATETIME())

		SELECT * FROM Search s INNER JOIN @Inserted i on s.ID = i.ID
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH
END

GO

CREATE PROCEDURE [dbo].[Search_End] 
(
	@SearchID int,
	@InputCount bigint = null,
	@LastInput varbinary(4096) = null
)
AS
BEGIN
	BEGIN TRY
		DECLARE @EndTime datetime2
		SET @EndTime = SYSDATETIME()
		
		UPDATE Search
		SET
			InputCount = @InputCount,
			LastInput = @LastInput,
			EndTime = @EndTime,
			Completed = 1,
			SearchSeconds = DATEDIFF(second, StartTime, @EndTime)
		WHERE ID = @SearchID

		SELECT * FROM Search s WHERE ID = @SearchID
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH
END

GO

/* Insert Algorithms */
INSERT INTO [Algorithm] (Name, [Length], TypeName, [Description])
--Cryptographic
SELECT 'BLAKE256', 256, 'Cryptographic', NULL UNION ALL
SELECT 'BLAKE512', 512, 'Cryptographic', NULL UNION ALL
SELECT 'ECOH', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'FSB', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'GOST', 256, 'Cryptographic', NULL UNION ALL
SELECT 'Grøstl', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'HAS160', 160, 'Cryptographic', NULL UNION ALL
SELECT 'HAVAL', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'JH', 512, 'Cryptographic', NULL UNION ALL
SELECT 'MD2', 128, 'Cryptographic', NULL UNION ALL
SELECT 'MD4', 128, 'Cryptographic', NULL UNION ALL
SELECT 'MD5', 128, 'Cryptographic', NULL UNION ALL
SELECT 'MD6', 512, 'Cryptographic', NULL UNION ALL
SELECT 'RadioGatún', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'RIPEMD', 128, 'Cryptographic', NULL UNION ALL
SELECT 'RIPEMD160', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'RIPEMD320', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'SHA1', 160, 'Cryptographic', NULL UNION ALL
SELECT 'SHA224', 224, 'Cryptographic', NULL UNION ALL
SELECT 'SHA256', 256, 'Cryptographic', NULL UNION ALL
SELECT 'SHA384', 384, 'Cryptographic', NULL UNION ALL
SELECT 'SHA512', 512, 'Cryptographic', NULL UNION ALL
SELECT 'Skein', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'SipHash', 64, 'Cryptographic', NULL UNION ALL
SELECT 'Snefru', NULL, 'Cryptographic', NULL UNION ALL
SELECT 'Spectral', 512, 'Cryptographic', NULL UNION ALL
SELECT 'SWIFFT', 512, 'Cryptographic', NULL UNION ALL
SELECT 'Tiger', 192, 'Cryptographic', NULL UNION ALL
SELECT 'Whirlpool', 512, 'Cryptographic', NULL

INSERT INTO [Algorithm] (Name, [Length], TypeName, [Description], Polynomial_Normal, Polynomial_Reversed)
--Cyclic Redundancy Check
SELECT 'CRC1',  1, 'Cyclic Redundancy Check', 'Parity Bit', 0x1, 0x1 UNION ALL
SELECT 'CRC4',  4, 'Cyclic Redundancy Check', 'ITU G.704', 0x3, 0xC UNION ALL
SELECT 'CRC5E', 5, 'Cyclic Redundancy Check', 'EPC Gen 2 RFID', 0x09, 0x12 UNION ALL
SELECT 'CRC5I', 5, 'Cyclic Redundancy Check', 'ITU G.704', 0x15, 0x15 UNION ALL
SELECT 'CRC5U', 5, 'Cyclic Redundancy Check', 'USB', 0x05, 0x14 UNION ALL

SELECT 'CRC6A', 6, 'Cyclic Redundancy Check', 'CDMA2000-A', 0x27, 0x39 UNION ALL
SELECT 'CRC6B', 6, 'Cyclic Redundancy Check', 'CDMA2000-B', 0x07, 0x38 UNION ALL
SELECT 'CRC6D', 6, 'Cyclic Redundancy Check', 'DARC', 0x19, 0x26 UNION ALL
SELECT 'CRC6I', 6, 'Cyclic Redundancy Check', 'ITU G.704', 0x03, 0x30 UNION ALL
 
SELECT 'CRC7',  7, 'Cyclic Redundancy Check', 'G.707, G.832, MMC, SD', 0x09, 0x48 UNION ALL
SELECT 'CRC7M', 7, 'Cyclic Redundancy Check', 'MVB IEC 60870-5', 0x65, 0x53 UNION ALL

SELECT 'CRC8',  8, 'Cyclic Redundancy Check', '', 0xD5, 0xAB UNION ALL
SELECT 'CRC8C', 8, 'Cyclic Redundancy Check', 'CCITT I.432.1', 0x07, 0xE0 UNION ALL
SELECT 'CRC8D', 8, 'Cyclic Redundancy Check', 'DARC', 0x39, 0x9C UNION ALL
SELECT 'CRC8M', 8, 'Cyclic Redundancy Check', 'Dallas/Maxim 1 Wire Bus', 0x31, 0x8C UNION ALL
SELECT 'CRC8S', 8, 'Cyclic Redundancy Check', 'SAE J1850 AES3', 0x1D, 0xB8 UNION ALL
SELECT 'CRC8W', 8, 'Cyclic Redundancy Check', 'WCDMA', 0x9B, 0xD9 UNION ALL

SELECT 'CRC10',  10, 'Cyclic Redundancy Check', 'ATM I.610', 0x233, 0x331 UNION ALL
SELECT 'CRC10C', 10, 'Cyclic Redundancy Check', 'CDMA2000', 0x3D9, 0x26F UNION ALL

SELECT 'CRC11',  11, 'Cyclic Redundancy Check', 'FlexRay', 0x385, 0x50E UNION ALL

SELECT 'CRC12',  12, 'Cyclic Redundancy Check', '', 0x80F, 0xF01 UNION ALL
SELECT 'CRC12C', 12, 'Cyclic Redundancy Check', 'CDMA2000', 0xF13, 0xC8F UNION ALL

SELECT 'CRC13B', 13, 'Cyclic Redundancy Check', 'BBC', 0x1CF5, 0x15E7 UNION ALL

SELECT 'CRC14D', 14, 'Cyclic Redundancy Check', 'DARC', 0x0805, 0x2804 UNION ALL

SELECT 'CRC15C', 15, 'Cyclic Redundancy Check', 'CAN', 0x4599, 0x4CD1 UNION ALL
SELECT 'CRC15M', 15, 'Cyclic Redundancy Check', 'MPT1327', 0x6815, 0x540B UNION ALL

SELECT 'CRC16',  16, 'Cyclic Redundancy Check', 'CCITT', 0x1021, 0x8408 UNION ALL
SELECT 'CRC16A', 16, 'Cyclic Redundancy Check', 'ARINC', 0xA02B, 0xD405 UNION ALL
SELECT 'CRC16C', 16, 'Cyclic Redundancy Check', 'CDMA2000', 0xC867, 0xE613 UNION ALL
SELECT 'CRC16D', 16, 'Cyclic Redundancy Check', 'DECT', 0x0589, 0x91A0 UNION ALL
SELECT 'CRC16I', 16, 'Cyclic Redundancy Check', 'IBM', 0x8005, 0xA001 UNION ALL
SELECT 'CRC16N', 16, 'Cyclic Redundancy Check', 'DNP', 0x3D65, 0xA6BC UNION ALL
SELECT 'CRC16S', 16, 'Cyclic Redundancy Check', 'SCSI DIF', 0x8BB7, 0xEDD1 UNION ALL

SELECT 'CRC17',  17, 'Cyclic Redundancy Check', 'CAN FD', 0x1685B, 0x1B42D UNION ALL
SELECT 'CRC21',  21, 'Cyclic Redundancy Check', 'CAN FD', 0x102899, 0x132281 UNION ALL
SELECT 'CRC24',  24, 'Cyclic Redundancy Check', 'FlexRay', 0x5D6DCB, 0xD3B6BA UNION ALL
SELECT 'CRC24R', 24, 'Cyclic Redundancy Check', 'Radix-64', 0x864CFB, 0xDF3261 UNION ALL
SELECT 'CRC30',  30, 'Cyclic Redundancy Check', 'CDMA', 0x2030B9C7, 0x38E74301 UNION ALL

SELECT 'CRC32',  32, 'Cyclic Redundancy Check', '', 0x04C11DB7, 0xEDB88320 UNION ALL
SELECT 'CRC32C', 32, 'Cyclic Redundancy Check', 'Castagnoli', 0x1EDC6F41, 0x82F63B78 UNION ALL
SELECT 'CRC32K', 32, 'Cyclic Redundancy Check', 'Koopman', 0x741B8CD7, 0xEB31D82E UNION ALL
SELECT 'CRC32Q', 32, 'Cyclic Redundancy Check', 'AIXM', 0x814141AB, 0xD5828281 UNION ALL

SELECT 'CRC40',  40, 'Cyclic Redundancy Check', 'GSM', 0x0004820009, 0x9000412000 UNION ALL

SELECT 'CRC64',  64, 'Cyclic Redundancy Check', 'ECMA', 0x42F0E1EBA9EA3693, 0xC96C5795D7870F42 UNION ALL
SELECT 'CRC64I', 64, 'Cyclic Redundancy Check', 'ISO', 0x000000000000001B, 0xD800000000000000

INSERT INTO [Algorithm] (Name, [Length], TypeName, [Description])
--Checksums
SELECT 'SUM', NULL, 'Checksum', NULL UNION ALL
SELECT 'SUM8', 8, 'Checksum', NULL UNION ALL
SELECT 'SUM16', 16, 'Checksum', NULL UNION ALL
SELECT 'SUM24', 24, 'Checksum', NULL UNION ALL
SELECT 'SUM32', 32, 'Checksum', NULL UNION ALL
SELECT 'Fletcher4', 4, 'Checksum', NULL UNION ALL
SELECT 'Fletcher8', 8, 'Checksum', NULL UNION ALL
SELECT 'Fletcher16', 16, 'Checksum', NULL UNION ALL
SELECT 'Fletcher32', 32, 'Checksum', NULL UNION ALL
SELECT 'Adler32', 32, 'Checksum', NULL UNION ALL
SELECT 'Xor8', 8, 'Checksum', NULL UNION ALL
SELECT 'Luhn', 4, 'Checksum', NULL UNION ALL
SELECT 'Verhoeff', 4, 'Checksum', NULL UNION ALL
SELECT 'Damm', 5, 'Checksum', NULL UNION ALL
--Non-Cryptographic
SELECT 'Pearson', 8, 'Non-Cryptographic', NULL UNION ALL
SELECT 'Buzhash', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'FNV', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'Zobrist', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'Jenkins', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'Java', 32, 'Non-Cryptographic', NULL UNION ALL
SELECT 'Bernstein', 32, 'Non-Cryptographic', NULL UNION ALL
SELECT 'elf64', 64, 'Non-Cryptographic', NULL UNION ALL
SELECT 'MurmurHash', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'SpookyHash', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'CityHash', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'nhash', NULL, 'Non-Cryptographic', NULL UNION ALL
SELECT 'xxHash', NULL, 'Non-Cryptographic', NULL

GO

/* Grant Permissions */

GRANT EXECUTE ON [dbo].[Algorithm_Insert] TO HashSearch
GRANT EXECUTE ON [dbo].[Similarity_Insert] TO HashSearch
GRANT EXECUTE ON [dbo].[Search_Start] TO HashSearch
GRANT EXECUTE ON [dbo].[Search_End] TO HashSearch

GO
