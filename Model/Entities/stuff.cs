using System;
namespace CovidTracker
{
    public class StuffData
    {
        public string CountryName { get; set; }
        public int ConfirmedCases { get; set; }
        public int Deaths { get; set; }
        //[DefaultValue(null)]
        public int? Recovered { get; set; }
    }
    public class StuffVaccineData
    {
        public DateTime Date { get; set; }
        public string CountryName { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}