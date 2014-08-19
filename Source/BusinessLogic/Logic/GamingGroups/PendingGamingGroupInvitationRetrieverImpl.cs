﻿using BusinessLogic.DataAccess;
using BusinessLogic.Models;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Logic.GamingGroups
{
    public class PendingGamingGroupInvitationRetrieverImpl : PendingGamingGroupInvitationRetriever
    {
        private DataContext dataContext;

        public PendingGamingGroupInvitationRetrieverImpl(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public virtual List<GamingGroupInvitation> GetPendingGamingGroupInvitations(ApplicationUser currentUser)
        {
            ApplicationUser user = dataContext.FindById<ApplicationUser>(currentUser.Id, currentUser);

            return dataContext.GetQueryable<GamingGroupInvitation>(currentUser)
                .Where(invitation => invitation.InviteeEmail == user.Email)
                .ToList();
        }
    }
}