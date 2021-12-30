using System;
namespace CovidTracker
{
    public class CSVCovidData
    {
        public string CountryName { get; set; }
        public int ConfirmedCases { get; set; }
        public int Deaths { get; set; }
        //[DefaultValue(null)]
        public int? Recovered { get; set; }
    }
    public class CSVVaccineData
    {
        public DateTime Date { get; set; }
        public string CountryName { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}