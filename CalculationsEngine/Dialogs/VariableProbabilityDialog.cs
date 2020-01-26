﻿// Copyright © 2020 Tomasz Pućka, Piotr Hełminiak, Marcin Rochowiak, Jakub Wąsik

// This file is part of Assess Extended.

// Assess Extended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.

// Assess Extended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with Assess Extended.  If not, see <http://www.gnu.org/licenses/>.

using System;
using CalculationsEngine.Maintenance;
using DataModel.Results;

namespace CalculationsEngine.Dialogs
{
    public class VariableProbabilityDialog : Dialog
    {
        public VariableProbabilityDialog(double lowerUtilityBoundary, double upperUtilityBoundary, DisplayObject displayObject)
            : base(lowerUtilityBoundary, upperUtilityBoundary, displayObject)
        {
        }

        protected override void SetInitialValues()
        {
            DisplayObject.X = (LowerUtilityBoundary + UpperUtilityBoundary) / 2;
            PointToAdd = new PartialUtilityValues(DisplayObject.X, DisplayObject.Lottery.NewPointUtility());
        }

        public override void SetInitialDialogValues(double lowerUtilityBoundary, double upperUtilityBoundary)
        {
            LowerUtilityBoundary = lowerUtilityBoundary;
            UpperUtilityBoundary = upperUtilityBoundary;
            SetInitialValues();
        }

        protected override void SetValuesIfLotteryChosen()
        {
            LowerUtilityBoundary = DisplayObject.X;
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfSureChosen()
        {
            UpperUtilityBoundary = DisplayObject.X;
            UpdateOtherMethodsPointToAdd();
        }

        protected override void SetValuesIfEqualChosen()
        {
            DisplayObject.PointsList.Add(PointToAdd);
            DisplayObject.PointsList.Sort((first, second) => first.X.CompareTo(second.X));
        }
    }
}