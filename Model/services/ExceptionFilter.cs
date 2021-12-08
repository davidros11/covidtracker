using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
namespace CovidTracker
{
    public class ExceptionFilter: IExceptionFilter
    { 
        public void OnException(ExceptionContext context)
        {
            Debug.Write(context.HttpContext.Request.Path);
            Debug.WriteLine(context.Exception.ToString());
            context.Result = new StatusCodeResult(500);
            context.ExceptionHandled = true;
        }
    }  
}