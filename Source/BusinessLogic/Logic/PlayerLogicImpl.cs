﻿using BusinessLogic.DataAccess;
using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Logic
{
    public class PlayerLogicImpl : BusinessLogic.Logic.PlayerLogic
    {
        private NerdScorekeeperDbContext dbContext = null;

        public PlayerLogicImpl(NerdScorekeeperDbContext context)
        {
            dbContext = context;
        }

        public Player GetPlayerDetails(int playerID)
        {
            return dbContext.Players
                .Where(player => player.Id == playerID)
                .Include("PlayerGameResults")
                .FirstOrDefault();

        }
    }
}