using System;
namespace CovidTracker
{
    /// <summary>
    /// Stores data from a covid data CSV file
    /// </summary>
    public class CSVCovidData
    {
        public string CountryName { get; set; }
        public int ConfirmedCases { get; set; }
        public int Deaths { get; set; }
        public int? Recovered { get; set; }
    }
}