using ApplicationCore.Constants;
using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Error response model for UnAuthorized error
    /// </summary>
    public class UnAuthorizedErrorResponseModel : ErrorResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public UnAuthorizedErrorResponseModel()
            : base(ErrorTypes.UnAuthorized, "Authorization denied.", HttpStatusCode.Unauthorized, "", "You are not authorized to access this resource.")
        {
        }
    }
}
