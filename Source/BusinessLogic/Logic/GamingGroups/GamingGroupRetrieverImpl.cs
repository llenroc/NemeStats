﻿using BusinessLogic.DataAccess;
using BusinessLogic.Logic.GameDefinitions;
using BusinessLogic.Logic.Players;
using BusinessLogic.Models;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Logic.GamingGroups
{
    public class GamingGroupRetrieverImpl : GamingGroupRetriever
    {
        private DataContext dataContext;
        private PlayerRetriever playerRetriever;
        private GameDefinitionRetriever gameDefinitionRetriever;

        public GamingGroupRetrieverImpl(DataContext dataContext, PlayerRetriever playerRetriever, GameDefinitionRetriever gameDefinitionRetriever)
        {
            this.dataContext = dataContext;
            this.playerRetriever = playerRetriever;
            this.gameDefinitionRetriever = gameDefinitionRetriever;
        }

        public GamingGroup GetGamingGroupDetails(int gamingGroupId)
        {
            GamingGroup gamingGroup = dataContext.FindById<GamingGroup>(gamingGroupId);

            gamingGroup.Players = playerRetriever.GetAllPlayers(gamingGroupId);

            gamingGroup.GameDefinitions = gameDefinitionRetriever.GetAllGameDefinitions(gamingGroupId);

            gamingGroup.OwningUser = dataContext.GetQueryable<ApplicationUser>()
                .Where(user => user.Id == gamingGroup.OwningUserId)
                .First();

            gamingGroup.GamingGroupInvitations = dataContext.GetQueryable<GamingGroupInvitation>()
                .Where(invitation => invitation.GamingGroupId == gamingGroup.Id)
                .ToList();

            AddRegisteredUserInfo(gamingGroup);

            return gamingGroup;
        }

        private void AddRegisteredUserInfo(GamingGroup gamingGroup)
        {
            List<string> registeredUserIds = (from gamingGroupInvitation in gamingGroup.GamingGroupInvitations
                                              select gamingGroupInvitation.RegisteredUserId).ToList();

            List<ApplicationUser> registeredUsers = dataContext.GetQueryable<ApplicationUser>()
                .Where(user => registeredUserIds.Contains(user.Id))
                .ToList();

            foreach (GamingGroupInvitation gamingGroupInvitation in gamingGroup.GamingGroupInvitations)
            {
                gamingGroupInvitation.RegisteredUser = registeredUsers
                    .Where(user => user.Id == gamingGroupInvitation.RegisteredUserId)
                    .FirstOrDefault();
            }
        }
    }
}
