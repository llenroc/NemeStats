﻿#region LICENSE
// NemeStats is a free website for tracking the results of board games.
//     Copyright (C) 2015 Jacob Gordon
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>
#endregion

using System;
using System.Data.Entity;
using BusinessLogic.DataAccess;
using BusinessLogic.DataAccess.Repositories;
using BusinessLogic.Models;
using System.Collections.Generic;
using System.Linq;
using BusinessLogic.Logic.BoardGameGeek;
using BusinessLogic.Models.Games;
using BusinessLogic.Models.Utility;

namespace BusinessLogic.Logic.GameDefinitions
{
    public class GameDefinitionRetriever : IGameDefinitionRetriever
    {
        private readonly IDataContext dataContext;
        private readonly IPlayerRepository playerRepository;  

        public GameDefinitionRetriever(IDataContext dataContext, IPlayerRepository playerRepository)
        {
            this.dataContext = dataContext;
            this.playerRepository = playerRepository;
        }

        public virtual IList<GameDefinitionSummary> GetAllGameDefinitions(int gamingGroupId, IDateRangeFilter dateRangeFilter = null)
        {
            if(dateRangeFilter == null)
            {
                dateRangeFilter = new BasicDateRangeFilter();
            }
            var returnValue = (from gameDefinition in dataContext.GetQueryable<GameDefinition>()
                where gameDefinition.GamingGroupId == gamingGroupId
                        && gameDefinition.Active
                select new GameDefinitionSummary
                {
                    Active = gameDefinition.Active,
                    BoardGameGeekGameDefinitionId = gameDefinition.BoardGameGeekGameDefinitionId,
                    Name = gameDefinition.Name,
                    Description = gameDefinition.Description,
                    GamingGroupId = gameDefinition.GamingGroupId,
                    Id = gameDefinition.Id,
                    PlayedGames = gameDefinition.PlayedGames.Where(
                        playedGame => playedGame.DatePlayed >= dateRangeFilter.FromDate
                            && playedGame.DatePlayed <= dateRangeFilter.ToDate)
                            .ToList(),
                    Champion = gameDefinition.Champion,
                    ChampionId = gameDefinition.ChampionId,
                    PreviousChampion = gameDefinition.PreviousChampion,
                    PreviousChampionId = gameDefinition.PreviousChampionId,
                    DateCreated = gameDefinition.DateCreated,
                    ThumbnailImageUrl = gameDefinition.BoardGameGeekGameDefinition == null ? null : gameDefinition.BoardGameGeekGameDefinition.Thumbnail
                })
                .OrderBy(game => game.Name)
                .ToList();

            AddPlayersToChampionData(returnValue);

            returnValue.ForEach( summary =>
            {
                summary.BoardGameGeekUri = BoardGameGeekUriBuilder.BuildBoardGameGeekGameUri(summary.BoardGameGeekGameDefinitionId);
                summary.Champion = summary.Champion ?? new NullChampion();
                summary.PreviousChampion = summary.PreviousChampion ?? new NullChampion();
                summary.TotalNumberOfGamesPlayed = summary.PlayedGames.Count;
            });
            return returnValue;
        }

        private void AddPlayersToChampionData(List<GameDefinitionSummary> gameDefinitionSummaries)
        {
            var playerIds = gameDefinitionSummaries.Select(x => x.Champion == null ? -1 : x.Champion.PlayerId).ToList()
                                                   .Union(gameDefinitionSummaries.Select(x => x.PreviousChampion?.PlayerId ?? -1).ToList());

            var players = dataContext.GetQueryable<Player>().Where(player => playerIds.Contains(player.Id)).ToList();

            foreach (var gameDefinitionSummary in gameDefinitionSummaries)
            {
                if (gameDefinitionSummary.Champion != null)
                {
                    gameDefinitionSummary.Champion.Player = players.FirstOrDefault(player => player.Id == gameDefinitionSummary.Champion.PlayerId);
                }

                if (gameDefinitionSummary.PreviousChampion != null)
                {
                    gameDefinitionSummary.PreviousChampion.Player = players.FirstOrDefault(player => player.Id == gameDefinitionSummary.PreviousChampion.PlayerId);
                }
            }
        }

        public virtual GameDefinitionSummary GetGameDefinitionDetails(int id, int numberOfPlayedGamesToRetrieve)
        {
            var gameDefinition = dataContext.GetQueryable<GameDefinition>()
                .Include(game => game.PlayedGames)
                .Include(game => game.Champion)
                .Include(game => game.Champion.Player)
                .Include(game => game.PreviousChampion)
                .Include(game => game.PreviousChampion.Player)
                .Include(game => game.GamingGroup)
                .Include(game => game.BoardGameGeekGameDefinition)
                .SingleOrDefault(game => game.Id == id);

            var gameDefinitionSummary =  new GameDefinitionSummary
                                                           {
                                                               Active = gameDefinition.Active,
                                                               BoardGameGeekGameDefinitionId = gameDefinition.BoardGameGeekGameDefinitionId,
                                                               BoardGameGeekUri = BoardGameGeekUriBuilder.BuildBoardGameGeekGameUri(gameDefinition.BoardGameGeekGameDefinitionId),
                                                               Name = gameDefinition.Name,
                                                               Description = gameDefinition.Description,
                                                               GamingGroup = gameDefinition.GamingGroup,
                                                               GamingGroupId = gameDefinition.GamingGroupId,
                                                               GamingGroupName = gameDefinition.GamingGroup.Name,
                                                               Id = gameDefinition.Id,
                                                               ThumbnailImageUrl = gameDefinition.BoardGameGeekGameDefinition?.Thumbnail,
                                                               TotalNumberOfGamesPlayed = gameDefinition.PlayedGames.Count,
                                                               AveragePlayersPerGame = gameDefinition.PlayedGames.Select(item => (decimal)item.NumberOfPlayers).DefaultIfEmpty(0M).Average(),
            Champion = gameDefinition.Champion ?? new NullChampion(),
                                                               PreviousChampion = gameDefinition.PreviousChampion ?? new NullChampion()
                                                           };

            var playedGames = AddPlayedGamesToTheGameDefinition(numberOfPlayedGamesToRetrieve, gameDefinitionSummary);
            var distinctPlayerIds = AddPlayerGameResultsToEachPlayedGame(playedGames);
            AddPlayersToPlayerGameResults(playedGames, distinctPlayerIds);
            gameDefinitionSummary.PlayerWinRecords = playerRepository.GetPlayerWinRecords(id);

            return gameDefinitionSummary;
        }

        private IList<PlayedGame> AddPlayedGamesToTheGameDefinition(
            int numberOfPlayedGamesToRetrieve,
            GameDefinitionSummary gameDefinitionSummary)
        {
            IList<PlayedGame> playedGames = dataContext.GetQueryable<PlayedGame>().Include(playedGame => playedGame.PlayerGameResults)
                .Where(playedGame => playedGame.GameDefinitionId == gameDefinitionSummary.Id)
                .OrderByDescending(playedGame => playedGame.DatePlayed)
                .Take(numberOfPlayedGamesToRetrieve)
                .ToList();

            foreach (var playedGame in playedGames)
            {
                playedGame.GameDefinition = gameDefinitionSummary;
            }

            gameDefinitionSummary.PlayedGames = playedGames;

            return playedGames;
        }

        private IList<int> AddPlayerGameResultsToEachPlayedGame(IList<PlayedGame> playedGames)
        {
            var playedGameIds = (from playedGame in playedGames
                                       select playedGame.Id).ToList();

            IList<PlayerGameResult> playerGameResults = dataContext.GetQueryable<PlayerGameResult>()
                .Where(playerGameResult => playedGameIds.Contains(playerGameResult.PlayedGameId))
                .OrderBy(playerGameResult => playerGameResult.GameRank)
                .ToList();

            var distinctPlayerIds = new HashSet<int>();

            foreach (var playedGame in playedGames)
            {
                playedGame.PlayerGameResults = (from playerGameResult in playerGameResults
                                                where playerGameResult.PlayedGameId == playedGame.Id
                                                select playerGameResult).ToList();

                ExtractDistinctListOfPlayerIds(distinctPlayerIds, playedGame);
            }

            return distinctPlayerIds.ToList();
        }

        private static void ExtractDistinctListOfPlayerIds(HashSet<int> distinctPlayerIds, PlayedGame playedGame)
        {
            foreach (var playerGameResult in playedGame.PlayerGameResults)
            {
                if (!distinctPlayerIds.Contains(playerGameResult.PlayerId))
                {
                    distinctPlayerIds.Add(playerGameResult.PlayerId);
                }
            }
        }

        private void AddPlayersToPlayerGameResults(IList<PlayedGame> playedGames, IList<int> distinctPlayerIds)
        {
            IList<Player> players = dataContext.GetQueryable<Player>()
                .Where(player => distinctPlayerIds.Contains(player.Id))
                .ToList();

            foreach (var playedGame in playedGames)
            {
                foreach (var playerGameResult in playedGame.PlayerGameResults)
                {
                    playerGameResult.Player = players.First(player => player.Id == playerGameResult.PlayerId);
                }
            }
        }


        public IList<GameDefinitionName> GetAllGameDefinitionNames(int gamingGroupId)
        {
            return dataContext.GetQueryable<GameDefinition>()
                              .Where(gameDefinition => gameDefinition.Active
                              && gameDefinition.GamingGroupId == gamingGroupId)
                              .Select(gameDefiniton => new GameDefinitionName
                              {
                                  BoardGameGeekGameDefinitionId = gameDefiniton.BoardGameGeekGameDefinitionId,
                                  Id = gameDefiniton.Id,
                                  Name = gameDefiniton.Name
                              }).ToList();
        }

        public List<TrendingGame> GetTrendingGames(int maxNumberOfGames, int numberOfDaysOfTrendingGames)
        {
            var startDate = DateTime.Now.Date.AddDays(-1 * numberOfDaysOfTrendingGames);
            return (from result in dataContext.GetQueryable<BoardGameGeekGameDefinition>()
                    select new TrendingGame
                    {
                        BoardGameGeekGameDefinitionId = result.Id,
                        Name = result.Name,
                        GamesPlayed = result.GameDefinitions.SelectMany(x => x.PlayedGames.Where(playedGame => playedGame.DatePlayed >= startDate)).Count(),
                        ThumbnailImageUrl = result.Thumbnail,
                        GamingGroupsPlayingThisGame = result.GameDefinitions.Count(
                            gameDefinition => gameDefinition.PlayedGames.Any(playedGame => playedGame.DatePlayed >= startDate))
                    })
                    .OrderByDescending(x => x.GamingGroupsPlayingThisGame)
                    .ThenByDescending(x => x.GamesPlayed)
                    .Take(maxNumberOfGames)
                    .ToList();
        }
    }
}
