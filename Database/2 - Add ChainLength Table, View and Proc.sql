
/* ChainLength Table */

CREATE TABLE [dbo].[ChainLength]
(
	ChainLengthID int PRIMARY KEY IDENTITY(1,1),

	AlgorithmID int not null,
	Input varbinary(4096) not null,
	ChainLength bigint not null,

	InsertDate datetime2 not null default(SYSDATETIME())
)

GO

ALTER TABLE [dbo].[ChainLength] ADD CONSTRAINT FK_ChainLength_HashAlgorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[HashAlgorithm](AlgorithmID)

GO

/* Alter HashAlgorithmsView */

ALTER VIEW [dbo].[HashAlgorithmView]
AS
WITH Chains (AlgorithmID, ChainCount, [MaxLength], MinLength, FixPointFound) 
AS
(
	SELECT 
		AlgorithmID, 
		count(*) as ChainCount, 
		max(ChainLength) as [MaxLength], 
		min(ChainLength) as MinLength,
		CASE 
			WHEN min(ChainLength) = 1 THEN 1
			ELSE 0
		END AS FixPointFound
	FROM
		ChainLength cl
	GROUP BY 
		AlgorithmID
), Similarity (AlgorithmID, SimilarityCount, MaxBitSimilarity, MaxByteSimilarity, FixPointFound) 
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
	si.FixPointFound as SimilarityFixPointFound,
	se.InputCount,
	se.SearchCount,
	se.TotalSeconds,
	cl.ChainCount,
	cl.[MaxLength],
	cl.MinLength,
	cl.FixPointFound as ChainLengthFixPointFound,
	CASE 
		WHEN si.FixPointFound = 1 OR cl.FixPointFound = 1 THEN 1
		ELSE 0 
	END AS FixPointFound
FROM 
	HashAlgorithm ha LEFT OUTER JOIN
	Similarity si on si.AlgorithmID = ha.AlgorithmID LEFT OUTER JOIN
	Chains cl on cl.AlgorithmID = ha.AlgorithmID LEFT OUTER JOIN
	Searching se on se.AlgorithmID = ha.AlgorithmID

GO

/* Create ChainLengthView */

CREATE VIEW [dbo].[ChainLengthView]
AS
SELECT
	cl.*,
	ha.Name as 'AlgorithmName',
	ha.HashLength,
	ha.TypeName
FROM 
	ChainLength cl LEFT OUTER JOIN
	HashAlgorithm ha on cl.AlgorithmID = ha.AlgorithmID

GO

/* Create ChainLength Procedures */

CREATE PROCEDURE [dbo].[ChainLength_Insert] 
(
	@AlgorithmName nvarchar(100),
	@Input varbinary(4096),
	@Length bigint
)
AS
BEGIN
	BEGIN TRY
		DECLARE @AlgorithmID int
		SELECT @AlgorithmID = AlgorithmID FROM HashAlgorithm WHERE Name = @AlgorithmName

		DECLARE @Inserted TABLE (ChainLengthID int)
		
		INSERT INTO ChainLength(AlgorithmID, Input, ChainLength)
		OUTPUT inserted.ChainLengthID INTO @Inserted
		VALUES (@AlgorithmID, @Input, @Length)

		SELECT * FROM ChainLength cl INNER JOIN @Inserted i on cl.ChainLengthID = i.ChainLengthID
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

