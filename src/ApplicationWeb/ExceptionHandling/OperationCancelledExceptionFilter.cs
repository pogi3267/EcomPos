using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Presentation.Api.ExceptionHandling
{
    /// <summary>
    /// Exception filter for operation cancelation. 
    /// </summary>
    public class OperationCancelledExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// On Exception
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is not OperationCanceledException) return;

            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(400);
        }
    }
}
