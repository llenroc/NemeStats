﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.Models.PlayedGame
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            Filter = new PlayedGamesFilterViewModel();
        }

        public IEnumerable<SelectListItem> GameDefinitions { get; set; }
        public PlayedGamesFilterViewModel Filter { get; set; }
    }
}