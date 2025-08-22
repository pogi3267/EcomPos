using ApplicationWeb.SubscribeTableDependencies;

namespace ApplicationWeb.MiddlewareExtensions
{
    public static class ApplicationBuilderExtension
    {
        public static void UseSqlTableDependency<T>(this IApplicationBuilder applicationBuilder, string connectionString)
            where T : ISubscribeTableDependency
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            var service = serviceProvider.GetService<T>();
            service.SubscribeTableDependency(connectionString);
        }
    }

    public class CustomSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sessionTimeoutBytes = context.Session.Get("SessionTimeout");
           if (sessionTimeoutBytes != null)
            {
                var sessionTimeoutMinutes = BitConverter.ToDouble(sessionTimeoutBytes);
                context.Session.SetString("LastAccessTime", DateTime.UtcNow.ToString());

                var lastAccessTimeStr = context.Session.GetString("LastAccessTime");
                if (DateTime.TryParse(lastAccessTimeStr, out DateTime lastAccessTime))
                {
                    if (DateTime.UtcNow.Subtract(lastAccessTime).TotalMinutes > sessionTimeoutMinutes)
                    {
                        context.Session.Clear(); // Clear session if expired
                        context.Response.Redirect("/Admin/Administration/Login"); // Redirect to login page
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}
