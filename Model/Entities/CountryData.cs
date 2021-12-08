using System;
using System.Collections.Generic;
namespace CovidTracker
{
    public class CountryData
    {
        public string Name { get; set; }
        public string Continent { get; set; }
        public List<PopulationData> PData { get; set; }
        public List<CountryDiseaseData> DiseaseData { get; set; }
        public List<CountryVaccineData> VaccineData { get; set; }
    }
}