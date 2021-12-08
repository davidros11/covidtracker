using System;
namespace CovidTracker
{
    public class CountryDiseaseData
    {
        public string Date { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int? Recovered { get; set; }
    }
    public class CountryVaccineData
    {
        public string Date { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int? Boosters { get; set; }
    }
}
