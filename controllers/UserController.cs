using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Session;
namespace CovidTracker
{
    public class UserController : ControllerBase
    {    
        private UserAccess _access;
        public UserController(UserAccess access)
        {
            _access = access;
        }
        [HttpPost]
        [Route("/User/Login")]
        public ActionResult Login([FromBody] Login login) 
        {
            if(login?.Password is null || login?.Name is null)
            {
                return BadRequest();
            }
            bool success = _access.AuthenticateUser(login);
            if(!success)
            {
                return NotFound();
            }
            HttpContext.Session.SetString("name",login.Name); 
            return Ok();
        }
        [HttpDelete]
        [Route("/User/Logout")]
        public ActionResult Logout() 
        {
            HttpContext.Session.Remove("name");
            return Ok();
        }
    }
}

