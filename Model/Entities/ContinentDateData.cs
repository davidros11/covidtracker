using System;
using System.ComponentModel;
namespace CovidTracker
{
    /// <summary>
    /// Shows data of a specific continent or the world at a given date
    /// </summary>
    public class ContinentDateData
    {
        public string Continent { get; set; }
        public long ConfirmedCases { get; set; }
        public int NewCases { get; set; }
        public int Deaths { get; set; }
        public long Recovered { get; set; }
        public long Population { get; set; }
        public long Vaccinated { get; set; }
        public long FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}