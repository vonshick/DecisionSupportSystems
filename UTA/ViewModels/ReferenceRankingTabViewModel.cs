﻿using UTA.Models.DataBase;
using UTA.Models.Tab;

namespace UTA.ViewModels
{
   public class ReferenceRankingTabViewModel : Tab
   {
      public ReferenceRankingTabViewModel(Criteria criteria, Alternatives alternatives)
      {
         Name = "Reference Ranking";
         Criteria = criteria;
         Alternatives = alternatives;
      }

      private Criteria Criteria { get; }
      private Alternatives Alternatives { get; }
   }
}