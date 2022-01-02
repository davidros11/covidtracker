using System;
using System.Collections.Generic;
namespace CovidTracker
{
    /// <summary>
    /// Shows population data for a country for a specific year
    /// </summary>
    public class PopulationData
    {
        public int Population { get; set; }
        public float Density {get; set; }
        public float? PovertyRate { get; set; }
        public float? DiabetesRate { get; set; }
        public float? MedianAge { get; set; }
    }
}