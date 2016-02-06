
USE HashSearch
GO

CREATE PROCEDURE [dbo].[Search_Update] 
(
	@SearchID int,
	@InputCount bigint = null,
	@LastInput varbinary(4096) = null
)
AS
BEGIN
	BEGIN TRY
		UPDATE Search
		SET
			InputCount = @InputCount,
			LastInput = @LastInput,
			SearchSeconds = DATEDIFF(second, StartTime, SYSDATETIME())
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

GRANT EXECUTE ON [dbo].[Search_Update] TO HashSearch
