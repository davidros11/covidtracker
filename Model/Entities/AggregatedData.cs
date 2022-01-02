using System;
using System.Collections.Generic;
namespace CovidTracker
{
    /// <summary>
    /// Shows data for a continent or the world
    /// </summary>
    public class AggregatedData
    {
        public Dictionary<string, long> PopulationsByYear { get; set; }
        public List<AreaDiseaseData> DiseaseData { get; set; }
        public List<AreaVaccineData> VaccineData { get; set; }
    }
}