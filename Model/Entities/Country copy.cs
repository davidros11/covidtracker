using System;
namespace CovidTracker
{
    public class AreaDiseaseData
    {
        public string Date { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int? Recovered { get; set; }
    }
    public class AreaVaccineData
    {
        public string Date { get; set; }
        public long Vaccinated { get; set; }
        public long FullyVaccinated { get; set; }
        public int? Boosters { get; set; }
    }
}
