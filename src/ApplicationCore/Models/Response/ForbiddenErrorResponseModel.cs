using ApplicationCore.Constants;
using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Error response model for UnAuthorized error
    /// </summary>
    public class ForbiddenErrorResponseModel : ErrorResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public ForbiddenErrorResponseModel()
            : base(ErrorTypes.Forbidden, "You don't have permission to access this resource.", HttpStatusCode.Forbidden, "", "You are not authorized to access this resource.")
        {
        }
    }
}
