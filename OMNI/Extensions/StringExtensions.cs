using System;

namespace OMNI.Extensions
{
    /// <summary>
    /// String Extensions Interaction Logic
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Check to see if a substring exists in a string ignoring case
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="check">String to check</param>
        /// <returns>bool </returns>
        public static bool NoCaseContains(this string source, string check)
        {
            return source.IndexOf(check, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
