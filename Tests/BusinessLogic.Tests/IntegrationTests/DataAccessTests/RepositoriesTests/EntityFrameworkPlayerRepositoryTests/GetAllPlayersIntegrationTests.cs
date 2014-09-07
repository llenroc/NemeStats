﻿using BusinessLogic.DataAccess;
using BusinessLogic.DataAccess.Repositories;
using BusinessLogic.Logic;
using BusinessLogic.Logic.Players;
using BusinessLogic.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Tests.IntegrationTests.DataAccessTests.RepositoriesTests.EntityFrameworkPlayerRepositoryTests
{
    [TestFixture]
    public class GetAllPlayersIntegrationTests : IntegrationTestBase
    {
        private DataContext dataContext;
        private PlayerRetrieverImpl playerRetriever;

        [SetUp]
        public void TestSetUp()
        {
            dataContext = new NemeStatsDataContext();
            playerRetriever = new PlayerRetrieverImpl(dataContext);
        }

        [Test]
        public void ItOnlyReturnsActivePlayers()
        {
            List<Player> players = playerRetriever.GetAllPlayers(testUserWithDefaultGamingGroup.CurrentGamingGroupId.Value);

            Assert.True(players.All(x => x.Active));
        }

        [Test]
        public void ItOnlyReturnsPlayersForTheGivenGamingGroupId()
        {
            List<Player> players = playerRetriever.GetAllPlayers(testUserWithDefaultGamingGroup.CurrentGamingGroupId.Value);

            Assert.True(players.All(x => x.GamingGroupId == testGamingGroup.Id));
        }

        [TearDown]
        public void TearDown()
        {
            dataContext.Dispose();
        }
    }
}
