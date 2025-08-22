namespace ApplicationCore.Constants
{
    /// <summary>
    /// Error Types
    /// </summary>
    public static class ErrorTypes
    {
        /// <summary>
        /// Inernal server error occured.
        /// </summary>
        public const string InternalServerError = "Internal Server Error.";

        /// <summary>
        /// Bad request by user.
        /// </summary>
        public const string BadRequest = "Bad Rquest.";

        /// <summary>
        /// User is not Authorized.
        /// </summary>
        public const string UnAuthorized = "User is not authorized.";

        /// <summary>
        /// 
        /// </summary>
        public const string Forbidden = "User is forbidden to the requested resource.";
    }
}
