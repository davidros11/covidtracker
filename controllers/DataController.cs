using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet]
        [Route("/Data/ByDate/PopulationData")]
        public ActionResult<Dictionary<string, DemographicData>> DataByDate(int year) 
        {
            return HandleDatabaseOutput(_access.GetPopulationData(year));
        }
        [HttpGet]
        [Route("/Data/ByDate/Countries")]
        public ActionResult<List<CountryDateData>> DataByDate(DateTime date, int threshold=14) 
        {
            return HandleDatabaseOutput(_access.GetDateData(date, threshold));
        }
        [HttpGet]
        [Route("/Data/ByDate/Continents")]
        public ActionResult<List<ContinentDateData>> ContinentData(DateTime date, int threshold=14) 
        {
            return HandleDatabaseOutput(_access.GetContinentDateData(date, threshold));
        }
        [HttpGet]
        [Route("/Data/ByDate/World")]
        public ActionResult<WorldDateData> WorldData(DateTime date, int threshhold=14) 
        {
            return HandleDatabaseOutput(_access.GetWorldData(date, threshhold));
        }
        [HttpGet]
        [Route("/Data/Countries/{id}")]
        public ActionResult<CountryData> CountryData(int id, DateTime? start, DateTime? end) 
        {
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetCountryData(id, s, e));
        }
        [HttpGet]
        [Route("/Data/Continents/{name}")]
        public ActionResult<AggregatedData> ContinentData(string name, DateTime? start, DateTime? end) 
        {
            name = name.Replace('_', ' ');
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetContinentData(name, s, e));
        }
        [HttpGet]
        [Route("/Data/World")]
        public ActionResult<AggregatedData> WorldData(DateTime? start, DateTime? end)
        {
            DateTime s = start ?? new DateTime(1000, 1, 1);
            DateTime e = end ?? new DateTime(2100, 1, 1);
            return HandleDatabaseOutput(_access.GetWorldData(s, e));
        }
        [HttpPost]
        [Route("/Data/AddDiseaseReport")]
        public ActionResult AddDiseaseReports([FromForm] IFormFile file)
        {
            string name = HttpContext.Session.GetString("name");
            if(name == null)
            {
                return new StatusCodeResult(401);
            }
            string[] dateStrs = file.FileName.Replace(".csv", "").Split('-');
            var dateNums = Array.ConvertAll(dateStrs, str => Int32.Parse(str));
            DateTime date = new DateTime(dateNums[2], dateNums[0], dateNums[1]);    
            using(Stream stream = file.OpenReadStream())
            _access.InsertCovidReports(date, stream);
            return Ok();
        }
        [HttpPost]
        [Route("/Data/AddVaccineReport")]
        public ActionResult AddVaccineReports(DateTime? date, [FromForm] IFormFile file)
        {
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

