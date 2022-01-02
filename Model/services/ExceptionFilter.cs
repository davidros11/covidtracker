using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
namespace CovidTracker
{
    public class ExceptionFilter: IExceptionFilter
    { 
        /// <summary>
        /// Sends 500 if an exception is caught here
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            Debug.Write(context.HttpContext.Request.Path);
            Debug.WriteLine(context.Exception.ToString());
            context.Result = new StatusCodeResult(500);
            context.ExceptionHandled = true;
        }
    }  
}