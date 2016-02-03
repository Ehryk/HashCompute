
select * from HashAlgorithmView

select InputCount * 1.0 / SearchSeconds as Rate, * from HashSearch

select * from HashSimilarity

select * from HashSearchView

select * from HashSearchView
where Completed = 0

select SearchID, DATEDIFF(second, StartTime, GETDATE()) from HashSearch
where Completed = 0

select * from CycleView
