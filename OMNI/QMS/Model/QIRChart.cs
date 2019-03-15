using OMNI.Extensions;
using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.QMS.Model
{
    public class QIRChart
    {
        #region Properties

        public string NCMCode { get; set; }
        public double Absolute { get; set; }
        public int Cumulative { get; set; }

        #endregion

        /// <summary>
        /// Retrieve NCM data for pareto charting
        /// </summary>
        /// <param name="type">Type of data to request</param>
        /// <param name="year">Year to retrieve data from</param>
        /// <param name="month">Month to retrieve data from</param>
        /// <param name="filter">Type of chart to create</param>
        /// <param name="workcenter">Workcenter to filter</param>
        /// <param name="percentage">All others percentage roll up</param>
        /// <param name="supplier">Supplier to filter</param>
        /// <returns>List<QIRChart> for charting</returns>
        public async static Task<List<QIRChart>> NCMDataAsync(string type, int year, int month, string filter, int? workcenter, int percentage, int? supplier)
        {
            var _tempList = new List<QIRChart>();
            var _dateFilter = month == 0 ? $"YEAR(a.QIRDate)={year}" : $"MONTH(a.QIRDate)={month} AND YEAR(a.QIRDate)={year}";
            var _wcFilter = workcenter == null || workcenter == 0 ? $"a.Origin>0" : $"a.Origin={workcenter}";
            percentage = percentage == 0 ? 80 : percentage;
            switch (type)
            {
                case "Count":
                    type = "COUNT(a.[NCMCode])";
                    break;
                case "Cost":
                    type = "SUM(a.[TotalCost])";
                    break;
            }
            if (filter == "InternalYTD" || filter == "InternalMTD")
            {
                filter = "a.SupplierID = 0";
            }
            else
            {
                filter = supplier == null || supplier == 0 ? "a.SupplierID > 0" : $"a.SupplierID = '{supplier}'";
            }
            var cmdText = $@"USE {App.DataBase};
                                SELECT
                                    {type} AS 'NCMCount', b.NCMCode, b.Summary,
	                                CASE WHEN SUM(a.[TotalCost]) = 0
	                                THEN ROUND(CAST(COUNT(a.[NCMCode]) AS float) / CAST(SUM(COUNT(a.[NCMCode])) OVER() AS float) * 100, 0)
	                                ELSE ROUND(CAST({type} AS float) / CAST(SUM({type}) OVER() AS float) * 100, 0) END AS 'Percent'
                                FROM
                                    dbo.qir_metrics_view a
                                RIGHT JOIN
                                    dbo.ncm b ON b.NCMCode = a.NCMCode
                                WHERE
                                    {_dateFilter} AND {filter} AND {_wcFilter}
                                GROUP BY
                                    b.NCMCode, b.Summary
                                ORDER BY
                                    NCMCount DESC;";
            try
            {
                using (SqlCommand cmd = new SqlCommand(cmdText, App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var other = 0;
                            var _abs = 0;
                            while (await reader.ReadAsync())
                            {
                                if (other <= percentage)
                                {
                                    other += reader.SafeGetInt32("Percent");
                                    _tempList.Add(new QIRChart { NCMCode = reader.SafeGetString("Summary"), Absolute = reader.SafeGetInt32("NCMCount"), Cumulative = other });
                                }
                                else
                                {
                                    _abs += reader.SafeGetInt32("NCMCount");
                                }
                            }
                            _tempList.Add(new QIRChart { NCMCode = "Others", Absolute = _abs, Cumulative = 100 });
                        }
                    }
                }
                return _tempList;
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Get QIR Numbers that match the selected data point in a QIR chart
        /// </summary>
        /// <param name="ncmSummary">NCM summary filter</param>
        /// <param name="workCenter">Origin work center filter</param>
        /// <param name="supplier">Supplier filter</param>
        /// <param name="month">Filter month</param>
        /// <param name="year">Filter year</param>]
        /// <param name="incoming">Should the table be filtered to only show incoming QIR's</param>
        /// <returns>DataTable of QIRNumbers that match the given select filter</returns>
        public static DataTable GetResults(string ncmSummary, int? workCenter, int? supplier, int month, int year, bool incoming)
        {
            try
            {
                using (DataTable _table = new DataTable())
                {
                    var _selectCmd = $@"USE {App.DataBase};
                                            SELECT
                                                a.[QIRNumber]
                                            FROM
                                                [qir_metrics_view] a
                                            WHERE
                                                a.[NCMCode]=(SELECT [NCMCode] FROM [dbo].[ncm] WHERE [Summary] = @p1) AND YEAR(a.[QIRDate]) = @p2";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(_selectCmd, App.SqlConAsync))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", ncmSummary);
                        adapter.SelectCommand.Parameters.AddWithValue("p2", year);
                        if (workCenter != 0)
                        {
                            adapter.SelectCommand.CommandText += " AND a.[Origin] = @p3";
                            adapter.SelectCommand.Parameters.AddWithValue("p3", workCenter);
                        }
                        if (month != 0)
                        {
                            adapter.SelectCommand.CommandText += " AND MONTH(a.[QIRDate]) = @p4";
                            adapter.SelectCommand.Parameters.AddWithValue("p4", month);
                        }
                        if (supplier != 0 && incoming)
                        {
                            adapter.SelectCommand.CommandText += " AND SupplierID = @p5";
                            adapter.SelectCommand.Parameters.AddWithValue("p5", supplier);
                        }
                        else if (incoming)
                        {
                            adapter.SelectCommand.CommandText += " AND SupplierID > 0";
                        }
                        adapter.Fill(_table);
                        return _table;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionWindow.Show("Unhandled Exception", e.Message, e);
                return null;
            }
        }
    }
}
