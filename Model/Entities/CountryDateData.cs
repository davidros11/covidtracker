using System;
using System.ComponentModel;
namespace CovidTracker
{
    /// <summary>
    /// Shows data of a specific country at a specific date
    /// </summary>
    public class CountryDateData
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public int ConfirmedCases { get; set; }
        public int NewCases { get; set; }
        public int Deaths { get; set; }
        public int? Recovered { get; set; }
        public int Population { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}