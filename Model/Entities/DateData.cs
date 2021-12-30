using System;
using System.ComponentModel;
namespace CovidTracker
{
    public class CountryDateData
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public int ConfirmedCases { get; set; }
        public int NewCases { get; set; }
        public int Deaths { get; set; }
        //[DefaultValue(null)]
        public int? Recovered { get; set; }
        public int Population { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
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