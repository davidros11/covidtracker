using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
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
        [Route("/Data/ByDate/Countries")]
        public ActionResult<List<CountryDateData>> DataByDate(DateTime date, int threshold=14) 
        {
            return HandleDatabaseOutput(_access.GetDateData(date, threshold));
        }
        [HttpGet]
        [Route("/Data/AllCountries")]
        public ActionResult<List<Country>> Countries(DateTime date) 
        {
            return HandleDatabaseOutput(_access.GetCountries());
        }
        [HttpGet]
        [Route("/Data/Countries/{id}")]
        public ActionResult<CountryData> CountryData(int id) 
        {
            return HandleDatabaseOutput(_access.GetCountryData(id));
        }
        // [HttpGet]
        // [Route("/Data/ByDate/Continents")]
        // public ActionResult<CountryData> ContinentData(int id, DateTime date, int threshhold=14) 
        // {
        //     return HandleDatabaseOutput(_access.GetCountryData(id, date, threshhold));
        // }
        [HttpGet]
        [Route("/Data/ByDate/World")]
        public ActionResult<WorldDateData> WorldData(DateTime date, int threshhold=14) 
        {
            return HandleDatabaseOutput(_access.GetWorldData(date, threshhold));
        }
    }
}

