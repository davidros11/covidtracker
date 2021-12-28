using System;
using System.Collections.Generic;
namespace CovidTracker
{
    public class CountryData
    {
        public string Name { get; set; }
        public string Continent { get; set; }
        public Dictionary<string, PopulationData> PData { get; set; }
        public List<AreaDiseaseData> DiseaseData { get; set; }
        public List<AreaVaccineData> VaccineData { get; set; }
    }

    public class AggregatedData
    {
        public Dictionary<string, long> PopulationsByYear { get; set; }
        public List<AreaDiseaseData> DiseaseData { get; set; }
        public List<AreaVaccineData> VaccineData { get; set; }
    }
}