using System;
using System.ComponentModel;
namespace CovidTracker
{
    /// <summary>
    /// Shows data for the world for a specific date
    /// </summary>
    public class WorldDateData
    {
        public int ConfirmedCases { get; set; }
        public int NewCases { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
        public long Population { get; set; }
        public long Vaccinated { get; set; }
        public long FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}