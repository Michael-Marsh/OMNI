using System;
using System.ComponentModel;

namespace OMNI.Extensions
{
    /// <summary>
    /// Enum Extensions Interaction Logic
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieve the description attribute set for an enum value
        /// </summary>
        /// <param name="e">Current Enum</param>
        /// <returns>DescriptionAttribute as string</returns>
        public static string GetDescription(this Enum e)
        {
            var das = (DescriptionAttribute[])e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return das != null && das.Length > 0 ? das[0].Description : e.ToString();
        }
    }
}
