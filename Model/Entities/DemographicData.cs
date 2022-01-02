using System;
using System.Collections.Generic;
namespace CovidTracker
{
    /// <summary>
    /// Stores demographic data for a specific country in a specific year
    /// </summary>
    public class DemographicData
    {
        public float Density {get; set; }
        public float? PovertyRate { get; set; }
        public float? DiabetesRate { get; set; }
        public float? MedianAge { get; set; }
    }
}