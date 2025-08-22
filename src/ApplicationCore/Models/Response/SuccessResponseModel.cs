using System.Net;

namespace ApplicationCore.Models
{
    /// <summary>
    /// Response model for success
    /// </summary>
    public class SuccessResponseModel
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="message">Success message</param>
        public SuccessResponseModel(string message)
        {
            Status = HttpStatusCode.OK;
            Title = "Success";
            Message = message;
        }

        /// <summary>
        /// Success status code (Default 200)
        /// </summary>
        public HttpStatusCode Status { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; }

    }
}
