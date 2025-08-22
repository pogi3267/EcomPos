using System;
using System.Net;

namespace ApplicationCore.Common
{
    public static class Common
    {
        public static string GetUniqueEntityNo(string prefix = "")
        {
            return prefix + DateTime.UtcNow.ToString("yyMMddffff"); ;
        }

        public static string GetIpAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); 
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            return ipAddress.ToString();
        }

    }
}
