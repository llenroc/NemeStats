USE NerdScorekeeper;

WITH ActiveAfterOneWeek (GamingGroupId)
AS 
(
  SELECT GamingGroup.[Id]
  FROM [dbo].[GamingGroup]
  WHERE EXISTS (SELECT 1 FROM PlayedGame WHERE PlayedGame.GamingGroupid = GamingGroup.id 
	AND PlayedGame.DateCreated > DATEADD(day, 7, GamingGroup.DateCreated))
),
ActiveAfterOneMonth (GamingGroupId)
AS 
(
  SELECT GamingGroup.[Id]
  FROM [dbo].[GamingGroup]
  WHERE EXISTS (SELECT 1 FROM PlayedGame WHERE PlayedGame.GamingGroupid = GamingGroup.id 
	AND PlayedGame.DateCreated > DATEADD(month, 1, GamingGroup.DateCreated))
),
ActiveAfterThreeMonths (GamingGroupId)
AS 
(
  SELECT GamingGroup.[Id]
  FROM [dbo].[GamingGroup]
  WHERE EXISTS (SELECT 1 FROM PlayedGame WHERE PlayedGame.GamingGroupid = GamingGroup.id 
	AND PlayedGame.DateCreated > DATEADD(month, 3, GamingGroup.DateCreated))
) 
 SELECT GamingGroup.id, GamingGroup.Name, GamingGroup.DateCreated, AspnetUsers.Email, 
 DATEDIFF(day, GamingGroup.DateCreated, GETDATE()) AS GamingGroupAge,
 MAX(PlayedGame.DateCreated) AS DateLastGamePlayed,
 COALESCE(DATEDIFF(day, MAX(PlayedGame.DateCreated), GETDATE()), 0) AS DaysSinceLastGameRecorded,
 COUNT(PlayedGame.Id) AS TotalGamesPlayed,
 COALESCE(DATEDIFF(day, GamingGroup.DateCreated, MAX(PlayedGame.DateCreated)), 0) AS NumberOfDaysGamingGroupWasActive,
 CASE WHEN EXISTS (SELECT 1 FROM ActiveAfterOneWeek WHERE GamingGroupId = GamingGroup.Id) THEN 1 ELSE 0 END AS ActiveAfter1Week,
 CASE WHEN EXISTS (SELECT 1 FROM ActiveAfterOneMonth WHERE GamingGroupId = GamingGroup.Id) THEN 1 ELSE 0 END AS ActiveAfter1Month,
 CASE WHEN EXISTS (SELECT 1 FROM ActiveAfterThreeMonths WHERE GamingGroupId = GamingGroup.Id) THEN 1 ELSE 0 END AS ActiveAfter3Months
 FROM GamingGroup 
 LEFT JOIN AspnetUsers ON GamingGroup.OwningUserId = AspNetUsers.Id
 LEFT JOIN PlayedGame ON PlayedGame.GamingGroupid = GamingGroup.Id
 GROUP BY GamingGroup.id, GamingGroup.Name, GamingGroup.DateCreated, AspnetUsers.Email
 ORDER BY ActiveAfter1Week DESC, ActiveAfter1Month DESC, ActiveAfter3Months DESC