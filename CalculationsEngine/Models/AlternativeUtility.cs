﻿using DataModel.Input;

namespace CalculationsEngine.Models
{
    public struct AlternativeUtility
    {
        public Alternative Alternative;
        public double Utility;

        public AlternativeUtility(Alternative alternative, double utility)
        {
            Alternative = alternative;
            Utility = utility;
        }
    }
}