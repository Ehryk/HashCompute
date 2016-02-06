
select * from AlgorithmView

select InputCount * 1.0 / SearchSeconds as Rate, * from Search

select * from Similarity

select * from SearchView

select * from SearchView
where Completed = 0

select ID, DATEDIFF(second, StartTime, GETDATE()) from Search
where Completed = 0

select * from CycleView
