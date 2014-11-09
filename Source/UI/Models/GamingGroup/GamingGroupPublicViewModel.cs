﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BusinessLogic.Models.Games;
using UI.Models.PlayedGame;
using UI.Models.Players;

namespace UI.Models.GamingGroup
{
    public class GamingGroupPublicViewModel
    {
        public int Id { get; set; }
        [DisplayName("Gaming Group Name")]
        public string Name { get; set; }
        public string OwningUserId { get; set; }
        [DisplayName("Owning User Name")]
        public string OwningUserName { get; set; }
        public IList<GameDefinitionSummary> GameDefinitionSummaries { get; set; }
        public IList<PlayerWithNemesisViewModel> Players { get; set; }
        public IList<PlayedGameDetailsViewModel> RecentGames { get; set; }
    }
}