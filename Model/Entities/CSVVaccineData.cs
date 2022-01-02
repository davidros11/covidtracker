using System;
namespace CovidTracker
{
    /// <summary>
    /// Stores data from a vaccine CSV file
    /// </summary>
    public class CSVVaccineData
    {
        public DateTime Date { get; set; }
        public string CountryName { get; set; }
        public int Vaccinated { get; set; }
        public int FullyVaccinated { get; set; }
        public int Boosters { get; set; }
    }
}