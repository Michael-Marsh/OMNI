using System;
using System.Data.SqlClient;

namespace OMNI.Extensions
{
    public static class SqlClientExtensions
    {
        /// <summary>
        /// Safely retrieve the string values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>string value or empty string</returns>
        public static string SafeGetString(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetString(reader.GetOrdinal(colName)) : string.Empty;
        }

        /// <summary>
        /// Safely retrieve the int values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>int value or 0</returns>
        public static int SafeGetInt32(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToInt32(reader.GetValue(reader.GetOrdinal(colName))) : 0;
        }

        /// <summary>
        /// Safely retrieve the double values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>double value or 0.00</returns>
        public static double SafeGetDouble(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToDouble(reader.GetValue(reader.GetOrdinal(colName))) : 0.00;
        }

        /// <summary>
        /// Safely retrieve the DateTime values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>DateTime value or '1999-01-01'</returns>
        public static DateTime SafeGetDateTime(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? reader.GetDateTime(reader.GetOrdinal(colName)) : DateTime.MinValue;
        }

        /// <summary>
        /// Safely retrieve the boolean values from a SqlDataReader object based on column
        /// </summary>
        /// <param name="reader">SqlDataReader object</param>
        /// <param name="colName">Name of the column to retrieve the data from</param>
        /// <returns>int value or 0</returns>
        public static bool SafeGetBoolean(this SqlDataReader reader, string colName)
        {
            return !reader.IsDBNull(reader.GetOrdinal(colName)) ? Convert.ToBoolean(reader.GetValue(reader.GetOrdinal(colName))) : false;
        }

        /// <summary>
        /// Safely add parameters to any SqlCommand object
        /// </summary>
        /// <param name="cmd">Current SqlCommand</param>
        /// <param name="param">Parameter in which you are using</param>
        /// <param name="value">The value to add to set to the inputed parameter</param>
        public static void SafeAddParemeters(this SqlCommand cmd, string param, object value)
        {
            if (value == null || (value.GetType() == typeof(DateTime) && Convert.ToDateTime(value) == DateTime.MinValue) || string.IsNullOrWhiteSpace(value.ToString()))
            {
                cmd.Parameters.AddWithValue(param, DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue(param, value);
            }
        }
    }
}
