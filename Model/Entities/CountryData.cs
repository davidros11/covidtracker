using System;
using System.Collections.Generic;
namespace CovidTracker
{
    /// <summary>
    /// Shows data of a specific country
    /// </summary>
    public class CountryData
    {
        public string Name { get; set; }
        public string Continent { get; set; }
        public Dictionary<string, PopulationData> PData { get; set; }
        public List<AreaDiseaseData> DiseaseData { get; set; }
        public List<AreaVaccineData> VaccineData { get; set; }
    }
}