using ApplicationCore.Constants;
using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Error response model for unhandled 500 errors
    /// </summary>
    public class InternalServerErrorResponseModel : ErrorResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="title">Error Title</param>
        /// <param name="traceId">Trace Id</param>
        /// <param name="errors">Error object</param>
        public InternalServerErrorResponseModel(string title, string traceId, object errors)
            : base(ErrorTypes.InternalServerError, title, HttpStatusCode.InternalServerError, traceId, errors)
        {
        }
    }
}
