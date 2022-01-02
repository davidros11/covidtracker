using System;
namespace CovidTracker
{
    /// <summary>
    /// Shows vaccine data of a specific country, continent or the world at a given date
    /// </summary>
    public class AreaVaccineData
    {
        public string Date { get; set; }
        public long Vaccinated { get; set; }
        public long FullyVaccinated { get; set; }
        public int? Boosters { get; set; }
    }
}
