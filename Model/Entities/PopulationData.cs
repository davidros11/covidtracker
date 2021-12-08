using System;
using System.Collections.Generic;
namespace CovidTracker
{
    public class PopulationData
    {
        public int Year { get; set; }
        public int Population { get; set; }
        public float Density {get; set; }
        public float? PovertyRate { get; set; }
        public float? DiabetesRate { get; set; }
        public float? MedianAge { get; set; }
    }
}