using Microsoft.AspNetCore.Routing;
using System;

namespace ApplicationCore.Utilities
{
    /// <summary>
    /// Slugify parameter transformer.
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <summary>
        /// Transform Outbound parameters
        /// </summary>
        /// <param name="value">value to transform</param>
        /// <returns></returns>
        public string TransformOutbound(object value)
        {
            if (value == null) { return null; }
            return value.ToString().Slugify();
        }
    }
}
