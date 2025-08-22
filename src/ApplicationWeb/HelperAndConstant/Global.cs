using Newtonsoft.Json;
using System.Net;

namespace ApplicationWeb.HelperAndConstant
{
    public static class Global
    {
        public static ApplicationCore.Entities.ApplicationUser.ApplicationUser GetCurrentUser()
        {
            IHttpContextAccessor context = new HttpContextAccessor();
            return JsonConvert.DeserializeObject<ApplicationCore.Entities.ApplicationUser.ApplicationUser>(context.HttpContext.Session.GetString("User"));
        }

        public static string GetUniqueEntityNo(string prefix = "")
        {
            return prefix + DateTime.UtcNow.ToString("yyMMddffff"); ;
        }

        public static string GetIpAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // `Dns.Resolve()` method is deprecated.
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            return ipAddress.ToString();
        }

    }

}

