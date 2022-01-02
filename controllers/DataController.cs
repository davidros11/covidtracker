using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
namespace CovidTracker
{
    public class DataController : ControllerBase
    {    
        private DataAccess _access;
        public DataController(DataAccess access)
        {
            _access = access;
        }
        [NonAction]
        private ActionResult<T> HandleDatabaseOutput<T>(T Output)
        {
            if(Output == null)
            {
                return NotFound();
            }
            return Output;
        }
        [HttpGet]
        [Route("/Data/AllCountries")]
        /// <summary>
        /// Gets a list of all countries, with id, name and continent
        /// </summary>
        /// <returns>countries list</returns>
        public ActionResult<List<Country>> Countries() 
        {
            return HandleDatabaseOutput(_access.GetCountries());
        }
        /// <summary>
        /// Gets the population data of all countries (median age, diabetes rate, population density, poverty rate)
        /// </summary>
        /// <param name="year">the year for the information</param>
        [HttpGet]
        [Route("/Data/ByDate/PopulationData")]
        public ActionResult<Dictionary<string, DemographicData>> DataByDate(int year) 
        {
            if(year < 1900)
            {
                return BadRequest("year too low");
            }
            return HandleDatabaseOutput(_access.GetDemographicData(year));
        }
        /// <summary>
        /// Get the covid relevant data of the countries by date
        /// (confirmed cases, deaths, recovered people, population, people vaccinated, fully vaccinated, number of boosters)
        /// </summary>
        /// <param name="date">the date for the data</param>
        /// <param name="threshold">the number of days over which new cases should be taken</param>
        [HttpGet]
        [Route("/Data/ByDate/Countries")]
        public ActionResult<List<CountryDateData>> DataByDate(DateTime date, int threshold=14) 
        {
            if(threshold < 0) { return BadRequest(); }
            if(date < new DateTime(1900, 1, 1))
            {
                return BadRequest("date too early");
            }
            return HandleDatabaseOutput(_access.GetCountryDateData((DateTime) date, threshold));
        }
        /// <summary>
        /// Get the covid relevant data of the continents by date
        /// (confirmed cases, deaths, recovered people, population, people vaccinated, fully vaccinated, number of boosters)
        /// </summary>
        /// <param name="date">the date for the data</param>
        /// <param name="threshold">the number of days over which new cases should be taken</param>
        [HttpGet]
        [Route("/Data/ByDate/Continents")]
        public ActionResult<List<ContinentDateData>> ContinentData(DateTime date, int threshold=14) 
        {
            if(threshold < 0) { return BadRequest(); }
             if(date < new DateTime(1900, 1, 1))
            {
                return BadRequest("date too early");
            }
            return HandleDatabaseOutput(_access.GetContinentDateData(date, threshold));
        }
        /// <summary>
        /// Gets the latest date with disease reports
        /// </summary>
        [HttpGet]
        [Route("Data/LatestDate")]
        public ActionResult<DateTime> LatestDate()
        {
            DateTime? date = _access.LatestDate();
            if(date == null) {  return NotFound(); }
            return _access.LatestDate();
        }
        /// <summary>
        /// Get the covid relevant data of the world by date
        /// (confirmed cases, deaths, recovered people, population, people vaccinated, fully vaccinated, number of boosters)
        /// </summary>
        /// <param name="date">the date for the data</param>
        /// <param name="threshold">the number of days over which new cases should be taken</param>
        [HttpGet]
        [Route("/Data/ByDate/World")]
        public ActionResult<WorldDateData> WorldData(DateTime date, int threshold=14) 
        {
            if(threshold < 0) { return BadRequest(); }
            if(date < new DateTime(1900, 1, 1))
            {
                return BadRequest("date too early");
            }
            return HandleDatabaseOutput(_access.GetWorldData(date, threshold));
        }
        [HttpGet]
        /// <summary>
        /// Shows information for a specific country
        /// (population by year, covid and vaccine reports by date)
        /// </summary>
        /// <param name="id">country id</param>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        [Route("/Data/Countries/{id}")]
        public ActionResult<CountryData> CountryData(int id, DateTime? start, DateTime? end) 
        {
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetCountryData(id, s, e));
        }
         /// <summary>
        /// Shows information for a specific continent
        /// (population by year, covid and vaccine reports by date)
        /// </summary>
        /// <param name="name">continent name</param>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        [HttpGet]
        [Route("/Data/Continents/{name}")]
        public ActionResult<AggregatedData> ContinentData(string name, DateTime? start, DateTime? end) 
        {
            name = name.Replace('_', ' ');
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetContinentData(name, s, e));
        }
        /// <summary>
        /// Shows information for the world
        /// (population by year, covid and vaccine reports by date)
        /// </summary>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        [HttpGet]
        [Route("/Data/World")]
        public ActionResult<AggregatedData> WorldData(DateTime? start, DateTime? end)
        {
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetWorldData(s, e));
        }
        /// <summary>
        /// Adds Covid reports by country and date"
        /// </summary>
        /// <param name="file">CSV file</param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Data/AddDiseaseReport")]
        public ActionResult AddDiseaseReports([FromForm] IFormFile file)
        {
            if(file == null)
            {
                return BadRequest();
            }
            // must be logged in
            string name = HttpContext.Session.GetString("name");
            if(name == null)
            {
                return new StatusCodeResult(401);
            }
            string dateStr = file.FileName.Replace(".csv", "");
            DateTime date = DateTime.ParseExact(dateStr, "MM-dd-yyyy", CultureInfo.InvariantCulture);  
            using(Stream stream = file.OpenReadStream())
            _access.InsertCovidReports(date, stream);
            return Ok();
        }
        /// <summary>
        /// Adds Vaccine reports by country and date"
        /// </summary>
        /// <param name="file">CSV file</param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Data/AddVaccineReport")]
        public ActionResult AddVaccineReports(DateTime? date, [FromForm] IFormFile file)
        {
            if(file == null)
            {
                return BadRequest();
            }
            // Must be logged in
            string name = HttpContext.Session.GetString("name");
            if(name == null)
            {
                return new StatusCodeResult(401);
            }
            using(Stream stream = file.OpenReadStream())
            {
                if(date is null)
                {
                    _access.InsertVaccineReports(stream);
                }
                else
                {
                    _access.InsertVaccineReports((DateTime) date, stream);
                }
            }
            return Ok();
        }
    }
}

