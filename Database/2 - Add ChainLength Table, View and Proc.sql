
/* Cycle Table */

CREATE TABLE [dbo].[Cycle]
(
	ID int PRIMARY KEY IDENTITY(1,1),

	AlgorithmID int not null,
	Input varbinary(4096) not null,
	[Length] bigint not null,

	InsertDate datetime2 not null default(sysdatetime())
)

GO

ALTER TABLE [dbo].[Cycle] ADD CONSTRAINT FK_Cycle_HashAlgorithm
FOREIGN KEY (AlgorithmID) REFERENCES [dbo].[HashAlgorithm](ID)

GO

/* Alter HashAlgorithmsView */

ALTER VIEW [dbo].[HashAlgorithmView]
AS
WITH Cycles (AlgorithmID, CycleCount, [MaxLength], MinLength, FixPointFound) 
AS
(
	SELECT 
		AlgorithmID, 
		count(*) as CycleCount, 
		max([Length]) as [MaxLength], 
		min([Length]) as MinLength,
		CASE 
			WHEN min([Length]) = 1 THEN 1
			ELSE 0
		END AS FixPointFound
	FROM
		Cycle c
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
	c.CycleCount,
	c.[MaxLength],
	c.MinLength,
	c.FixPointFound as ChainLengthFixPointFound,
	CASE 
		WHEN si.FixPointFound = 1 OR c.FixPointFound = 1 THEN 1
		ELSE 0 
	END AS FixPointFound
FROM 
	HashAlgorithm ha LEFT OUTER JOIN
	Similarity si on si.AlgorithmID = ha.ID LEFT OUTER JOIN
	Cycles c on c.AlgorithmID = ha.ID LEFT OUTER JOIN
	Searching se on se.AlgorithmID = ha.ID

GO

/* Create ChainLengthView */

CREATE VIEW [dbo].[CycleView]
AS
SELECT
	c.*,
	ha.Name as 'AlgorithmName',
	ha.HashLength,
	ha.TypeName
FROM 
	Cycle c LEFT OUTER JOIN
	HashAlgorithm ha on c.AlgorithmID = ha.ID

GO

/* Create ChainLength Procedures */

CREATE PROCEDURE [dbo].[Cycle_Insert] 
(
	@AlgorithmName nvarchar(100),
	@Input varbinary(4096),
	@Length bigint
)
AS
BEGIN
	BEGIN TRY
		DECLARE @AlgorithmID int
		SELECT @AlgorithmID = ID FROM HashAlgorithm WHERE Name = @AlgorithmName

		DECLARE @Inserted TABLE (ID int)
		
		INSERT INTO Cycle(AlgorithmID, Input, [Length])
		OUTPUT inserted.ID INTO @Inserted
		VALUES (@AlgorithmID, @Input, @Length)

		SELECT c.* FROM Cycle c INNER JOIN @Inserted i on c.ID = i.ID
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

GRANT EXECUTE ON [dbo].[Cycle_Insert] TO HashSearch
