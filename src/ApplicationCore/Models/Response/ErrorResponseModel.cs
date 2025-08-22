using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Error 
    /// </summary>
    public class ErrorResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ErrorResponseModel()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="type">Error type</param>
        /// <param name="title">Error title</param>
        /// <param name="status">Status code</param>
        /// <param name="traceId">Trace id</param>
        /// <param name="errors">Errors</param>
        public ErrorResponseModel(string type, string title, HttpStatusCode status, string traceId, object errors)
        {
            Type = type;
            Title = title;
            Status = status;
            TraceId = traceId;
            Errors = errors;
        }

        /// <summary>
        /// Error Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Http status code
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Trace Id
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Error object, can be a single error, or list of errors.
        /// </summary>
        public object Errors { get; set; }
    }
}
