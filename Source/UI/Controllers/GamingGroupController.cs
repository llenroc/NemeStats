﻿using BusinessLogic.DataAccess;
using BusinessLogic.DataAccess.Repositories;
using BusinessLogic.Models;
using BusinessLogic.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Filters;
using UI.Models.GamingGroup;
using UI.Transformations;

namespace UI.Controllers
{
    public partial class GamingGroupController : Controller
    {
        internal NemeStatsDbContext db;
        internal GamingGroupToGamingGroupViewModelTransformation gamingGroupToGamingGroupViewModelTransformation;
        internal GamingGroupRepository gamingGroupRepository;

        public GamingGroupController(
            NemeStatsDbContext dbContext,
            GamingGroupToGamingGroupViewModelTransformation gamingGroupToGamingGroupViewModelTransformation,
            GamingGroupRepository gamingGroupRespository)
        {
            this.db = dbContext;
            this.gamingGroupToGamingGroupViewModelTransformation = gamingGroupToGamingGroupViewModelTransformation;
            this.gamingGroupRepository = gamingGroupRespository;
        }

        //
        // GET: /GamingGroup
        [UserContextActionFilter]
        public virtual ActionResult Index(UserContext userContext)
        {
            //TODO should redirect to some other action if the user doesn't have a gaming group (rather than NPE here)
            GamingGroup gamingGroup = gamingGroupRepository.GetGamingGroupDetails(userContext.GamingGroupId.Value, userContext);
            GamingGroupViewModel viewModel = gamingGroupToGamingGroupViewModelTransformation.Build(gamingGroup);

            return View(MVC.GamingGroup.Views.Index, viewModel);
        }

        //
        // POST: /GamingGroup/Delete/5
        [HttpPost]
        public virtual ActionResult GrantAccess(string email, UserContext userContext)
        {
            try
            {
                return RedirectToAction(MVC.GamingGroup.ActionNames.Index);
            }
            catch
            {
                return View();
            }
        }
    }
}
