using ApplicationCore.Constants;
using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Error response model for bad request.
    /// </summary>
    public class BadRequestResponseModel : ErrorResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="title">Error Title</param>
        /// <param name="errors">Error object</param>
        public BadRequestResponseModel(string title, object errors)
            : base(ErrorTypes.BadRequest, title, HttpStatusCode.BadRequest, "", errors)
        {
        }
    }
}
