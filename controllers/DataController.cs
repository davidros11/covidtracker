using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
namespace Submit_System.Controllers
{
    public class DataController : ControllerBase
    {    
        [HttpGet]
        [Route("/hello")]
        public ActionResult<string> hello()
        {
            return "Hello world";
        }
    }
}

