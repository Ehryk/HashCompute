
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

/* HashAlgorithms Table */

CREATE TABLE [dbo].[HashAlgorithm]
(
	AlgorithmID int PRIMARY KEY IDENTITY(1,1),

	Name nvarchar(100) not null,
	HashLength int null,
	TypeName nvarchar(100) null,

	InsertUser nvarchar(128) not null default(suser_sname()),
	InsertDate datetime2 not null default(SYSDATETIME()),
	UpdateUser nvarchar(128) null,
	UpdateDate datetime2 null
)

GO

CREATE TRIGGER [dbo].[trg_HashAlgorithm_Audit] ON [dbo].[HashAlgorithm]
FOR INSERT, UPDATE
AS
BEGIN

	IF NOT EXISTS (SELECT 1 FROM deleted)
	BEGIN
		-- Insert
		update	HashAlgorithm
		set		InsertUser = suser_sname(),
				InsertDate = sysdatetime()
		from	HashAlgorithm
		join	Inserted on HashAlgorithm.AlgorithmID = Inserted.AlgorithmID
	END
	ELSE
	BEGIN
		-- Update
		update	HashAlgorithm
		set		UpdateUser = suser_sname(),
				UpdateDate = sysdatetime()
		from	HashAlgorithm
		join	Inserted on HashAlgorithm.AlgorithmID = Inserted.AlgorithmID
	END

END

GO

/* HashSearches Table */

CREATE TABLE [dbo].[HashSearch]
(
	SearchID int PRIMARY KEY IDENTITY(1,1),
	AlgorithmID int not null,

	MachineName nvarchar(100) null,
	InputCount bigint not null default 0,
	Seed varbinary(4096) null,
	SearchMode nvarchar(100) null,
	LastInput varbinary(4096) null,
	Completed bit not null default 0,
	
	StartTime datetime2 not null default(SYSDATETIME()),
	EndTime datetime2 null,
	SearchSeconds int null,

	InsertUser nvarchar(128) not null default(suser_sname()),
	InsertDate datetime2 not null default(SYSDATETIME()),
	UpdateUser nvarchar(128) null,
	UpdateDate datetime2 null
)

GO

ALTER TABLE [dbo].[HashSearch] ADD CONSTRAINT FK_HashSearch_HashAlgorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[HashAlgorithm](AlgorithmID)

GO

/* HashSimilarity Table */

CREATE TABLE [dbo].[HashSimilarity]
(
	SimilarityID int PRIMARY KEY IDENTITY(1,1),

	AlgorithmID int not null,
	Input varbinary(4096) not null,
	Result varbinary(4096) not null,
	
	BitSimilarity int null,
	ByteSimilarity int null,
	FixPoint bit not null default 0,

	InsertDate datetime2 not null default(SYSDATETIME())
)

GO

ALTER TABLE [dbo].[HashSimilarity] ADD CONSTRAINT FK_HashSimilarity_HashAlgorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[HashAlgorithm](AlgorithmID)

GO

/* Create HashAlgorithmsView */

CREATE VIEW [dbo].[HashAlgorithmView]
AS
WITH Similarity (AlgorithmID, SimilarityCount, MaxBitSimilarity, MaxByteSimilarity, FixPointFound) 
AS
(
	SELECT 
		AlgorithmID, 
		count(*) as SimilarityCount, 
		max(BitSimilarity) as MaxBitSimilarity, 
		max(ByteSimilarity) as MaxByteSimilarity,
		max(CAST(FixPoint AS tinyint)) as FixPointFound
	FROM
		HashSimilarity s
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
		HashSearch s
	GROUP BY AlgorithmID
)
SELECT
	ha.*,
	si.SimilarityCount, 
	si.MaxBitSimilarity,
	si.MaxByteSimilarity,
	si.FixPointFound,
	se.InputCount,
	se.SearchCount,
	se.TotalSeconds
FROM 
	HashAlgorithm ha LEFT OUTER JOIN
	Similarity si on si.AlgorithmID = ha.AlgorithmID LEFT OUTER JOIN
	Searching se on se.AlgorithmID = ha.AlgorithmID

GO

/* Create HashSearchView */

CREATE VIEW [dbo].[HashSearchView]
AS
SELECT
	hs.*,
	1.0 * hs.InputCount / hs.SearchSeconds as 'Rate',
	ha.Name as 'AlgorithmName',
	ha.HashLength,
	ha.TypeName
FROM 
	HashSearch hs LEFT OUTER JOIN
	HashAlgorithm ha on hs.AlgorithmID = ha.AlgorithmID

GO

/* Create HashSimilarityView */

CREATE VIEW [dbo].[HashSimilarityView]
AS
SELECT
	hs.*,
	CASE 
		WHEN BitSimilarity = HashLength THEN 1
		ELSE 0
		END as 'FullBitSimilarity',
	ha.Name as 'AlgorithmName',
	ha.HashLength,
	ha.TypeName
FROM 
	HashSimilarity hs LEFT OUTER JOIN
	HashAlgorithm ha on hs.AlgorithmID = ha.AlgorithmID

GO

/* Create Procedures */

CREATE PROCEDURE [dbo].[HashAlgorithm_Insert] 
(
	@Name nvarchar(100),
	@HashLength int = null,
	@TypeName nvarchar(100) = null
)
AS
BEGIN
	BEGIN TRY
		DECLARE @Inserted TABLE (AlgorithmID int)
		
		INSERT INTO HashAlgorithm (Name, HashLength, TypeName)
		OUTPUT inserted.AlgorithmID INTO @Inserted
		VALUES (@Name, @HashLength, @TypeName)

		SELECT * FROM HashAlgorithm a INNER JOIN @Inserted i on a.AlgorithmID = i.AlgorithmID
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

CREATE PROCEDURE [dbo].[HashSimilarity_Insert] 
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
		SELECT @AlgorithmID = AlgorithmID FROM HashAlgorithm WHERE Name = @AlgorithmName

		IF (@FixPoint is null)
		BEGIN
			SET @FixPoint = 0
			IF (@Input = @Result)
				SET @FixPoint = 1
		END

		DECLARE @Inserted TABLE (SimilarityID int)
		
		INSERT INTO HashSimilarity(AlgorithmID, Input, Result, BitSimilarity, ByteSimilarity, FixPoint)
		OUTPUT inserted.SimilarityID INTO @Inserted
		VALUES (@AlgorithmID, @Input, @Result, @BitSimilarity, @ByteSimilarity, @FixPoint)

		SELECT * FROM HashSimilarity s INNER JOIN @Inserted i on s.SimilarityID = i.SimilarityID
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

CREATE PROCEDURE [dbo].[HashSearch_Start] 
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
		SELECT @AlgorithmID = AlgorithmID FROM HashAlgorithm WHERE Name = @AlgorithmName

		DECLARE @Inserted TABLE (SearchID int)
		
		INSERT INTO HashSearch(AlgorithmID, MachineName, SearchMode, Seed, StartTime)
		OUTPUT inserted.SearchID INTO @Inserted
		VALUES (@AlgorithmID, @MachineName, @SearchMode, @Seed, SYSDATETIME())

		SELECT * FROM HashSearch s INNER JOIN @Inserted i on s.SearchID = i.SearchID
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

CREATE PROCEDURE [dbo].[HashSearch_End] 
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
		
		UPDATE HashSearch
		SET
			InputCount = @InputCount,
			LastInput = @LastInput,
			EndTime = @EndTime,
			Completed = 1,
			SearchSeconds = DATEDIFF(second, StartTime, @EndTime)
		WHERE SearchID = @SearchID

		SELECT * FROM HashSearch s WHERE SearchID = @SearchID
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
INSERT INTO HashAlgorithm (Name, HashLength, TypeName)
--Cryptographic
SELECT 'BLAKE256', 256, 'Cryptographic' UNION ALL
SELECT 'BLAKE512', 512, 'Cryptographic' UNION ALL
SELECT 'ECOH', NULL, 'Cryptographic' UNION ALL
SELECT 'FSB', NULL, 'Cryptographic' UNION ALL
SELECT 'GOST', 256, 'Cryptographic' UNION ALL
SELECT 'Grøstl', NULL, 'Cryptographic' UNION ALL
SELECT 'HAS160', 160, 'Cryptographic' UNION ALL
SELECT 'HAVAL', NULL, 'Cryptographic' UNION ALL
SELECT 'JH', 512, 'Cryptographic' UNION ALL
SELECT 'MD2', 128, 'Cryptographic' UNION ALL
SELECT 'MD4', 128, 'Cryptographic' UNION ALL
SELECT 'MD5', 128, 'Cryptographic' UNION ALL
SELECT 'MD6', 512, 'Cryptographic' UNION ALL
SELECT 'RadioGatún', NULL, 'Cryptographic' UNION ALL
SELECT 'RIPEMD', 128, 'Cryptographic' UNION ALL
SELECT 'RIPEMD160', NULL, 'Cryptographic' UNION ALL
SELECT 'RIPEMD320', NULL, 'Cryptographic' UNION ALL
SELECT 'SHA1', 160, 'Cryptographic' UNION ALL
SELECT 'SHA224', 224, 'Cryptographic' UNION ALL
SELECT 'SHA256', 256, 'Cryptographic' UNION ALL
SELECT 'SHA384', 384, 'Cryptographic' UNION ALL
SELECT 'SHA512', 512, 'Cryptographic' UNION ALL
SELECT 'Skein', NULL, 'Cryptographic' UNION ALL
SELECT 'SipHash', 64, 'Cryptographic' UNION ALL
SELECT 'Snefru', NULL, 'Cryptographic' UNION ALL
SELECT 'Spectral', 512, 'Cryptographic' UNION ALL
SELECT 'SWIFFT', 512, 'Cryptographic' UNION ALL
SELECT 'Tiger', 192, 'Cryptographic' UNION ALL
SELECT 'Whirlpool', 512, 'Cryptographic' UNION ALL
--Cyclic Redundancy Check
SELECT 'BSD', 16, 'Cyclic Redundancy Check' UNION ALL
SELECT 'Checksum', 32, 'Cyclic Redundancy Check' UNION ALL
SELECT 'CRC16', 16, 'Cyclic Redundancy Check' UNION ALL
SELECT 'CRC32', 32, 'Cyclic Redundancy Check' UNION ALL
SELECT 'CRC64', 64, 'Cyclic Redundancy Check' UNION ALL
SELECT 'SYSV', 16, 'Cyclic Redundancy Check' UNION ALL
--Checksums
SELECT 'SUM', NULL, 'Checksum' UNION ALL
SELECT 'SUM8', 8, 'Checksum' UNION ALL
SELECT 'SUM16', 16, 'Checksum' UNION ALL
SELECT 'SUM24', 24, 'Checksum' UNION ALL
SELECT 'SUM32', 32, 'Checksum' UNION ALL
SELECT 'Fletcher4', 4, 'Checksum' UNION ALL
SELECT 'Fletcher8', 8, 'Checksum' UNION ALL
SELECT 'Fletcher16', 16, 'Checksum' UNION ALL
SELECT 'Fletcher32', 32, 'Checksum' UNION ALL
SELECT 'Adler32', 32, 'Checksum' UNION ALL
SELECT 'Xor8', 8, 'Checksum' UNION ALL
SELECT 'Luhn', 4, 'Checksum' UNION ALL
SELECT 'Verhoeff', 4, 'Checksum' UNION ALL
SELECT 'Damm', 5, 'Checksum' UNION ALL
--Non-Cryptographic
SELECT 'Pearson', 8, 'Non-Cryptographic' UNION ALL
SELECT 'Buzhash', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'FNV', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'Zobrist', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'Jenkins', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'Java', 32, 'Non-Cryptographic' UNION ALL
SELECT 'Bernstein', 32, 'Non-Cryptographic' UNION ALL
SELECT 'elf64', 64, 'Non-Cryptographic' UNION ALL
SELECT 'MurmurHash', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'SpookyHash', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'CityHash', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'nhash', NULL, 'Non-Cryptographic' UNION ALL
SELECT 'xxHash', NULL, 'Non-Cryptographic'

GO

/* Grant Permissions */

GRANT EXECUTE ON [dbo].[HashAlgorithm_Insert] TO HashSearch
GRANT EXECUTE ON [dbo].[HashSimilarity_Insert] TO HashSearch
GRANT EXECUTE ON [dbo].[HashSearch_Start] TO HashSearch
GRANT EXECUTE ON [dbo].[HashSearch_End] TO HashSearch

GO
