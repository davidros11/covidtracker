using System;
namespace CovidTracker
{
    /// <summary>
    /// Shows covid data of a specific country, continent or the world at a given date
    /// </summary>
    public class AreaDiseaseData
    {
        public string Date { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int? Recovered { get; set; }
    }
}
