using ApplicationCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Presentation.Api.ExceptionHandling
{
    /// <summary>
    /// This handles all unhandled exceptions
    /// </summary>
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<UnhandledExceptionFilter> _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        public UnhandledExceptionFilter(ILogger<UnhandledExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override void OnException(ExceptionContext context)
        {
            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var actionDescriptor = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor;
            Type controllerType = actionDescriptor.ControllerTypeInfo;
            _logger.LogError(context.Exception, "Unhandled Exception in controller: {controller}.{method}", controllerType.Name, actionDescriptor.MethodInfo.Name);

            var error = context.Exception;
            while (error?.InnerException != null)
            {
                error = error.InnerException;
            }
            var responseText = JsonConvert.SerializeObject(new InternalServerErrorResponseModel(error?.Message, error?.StackTrace, error?.Data));

            context.ExceptionHandled = true;
            var objectResult = new ObjectResult(responseText)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.Result = objectResult; 
        }
    }
}
