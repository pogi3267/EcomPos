using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Null or Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Not Null or Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Null or Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NullOrEmpty(this StringValues value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Not Null or Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool NotNullOrEmpty(this StringValues value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Not Null or Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this IQueryCollection query, string key)
        {
            var value = query.FirstOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value;
            return string.IsNullOrEmpty(value) ? "" : value.ToString();
        }

        /// <summary>
        /// Slugify string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Slugify(this string value)
        {
            var s = value.ToLower();
            s = Regex.Replace(s, @"[^a-z0-9\s-]", "");                      // remove invalid characters
            s = Regex.Replace(s, @"\s+", " ").Trim();                       // single space
            s = s.Substring(0, s.Length <= 45 ? s.Length : 45).Trim();      // cut and trim
            s = Regex.Replace(s, @"\s", "-");                               // insert hyphens
            return s.ToLower();
        }

        public static Tuple<bool, DateTimeOffset> IsDateTime(this string value)
        {
            var isDateTime = DateTimeOffset.TryParse(value, out DateTimeOffset date);
            return new Tuple<bool, DateTimeOffset>(isDateTime, date);
        }

        public static string ReplaceBoolToSql(this string value)
        {
            if (value.Contains("true", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Replace("true", "1");
            }
            else if (value.Contains("yes", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Replace("yes", "1");
            }
            else if (value.Contains("false", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Replace("false", "0");
            }
            else if (value.Contains("no", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Replace("no", "0");
            }

            return value;
        }

    }
}
