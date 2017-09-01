using MySql.Data.MySqlClient;
using OMNI.Helpers;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns>List<QIRChart> for charting</returns>

        public async static Task<List<QIRChart>> NCMDataAsync(string type, int year, int month, string filter, int? workcenter, int percentage)
        {
            var _ncmCountData = new List<QIRChart>();
            var absoluteList = new List<NCM>();
            var _dateFilter = string.Empty;
            var _workcenterFilter = string.Empty;
            var sqlCmd = string.Empty;
            switch (type)
            {
                case "Count":
                    type = "COUNT(q.`NCMCode`)";
                    break;
                case "Cost":
                    type = "SUM(q.`TotalCost`)";
                    break;
            }
            _dateFilter = month == 0
                ? $"YEAR(q.`QIRDate`)={year}"
                : $"MONTH(q.`QIRDate`)={month} AND YEAR(q.`QIRDate`)={year}";
            filter = filter == "InternalYTD" || filter == "InternalMTD"
                ? "q.`SupplierID`=0"
                : "q.`SupplierID`>0";
            _workcenterFilter = workcenter == null || workcenter == 0
                ? $"q.`Origin`>0"
                : $"q.`Origin`={workcenter}";
            sqlCmd = $"SELECT q.`NCMCode`, {type}, n.`NCMCode`, n.`Summary` FROM `{App.Schema}`.`qir_metrics_view` AS q, `{App.Schema}`.`ncm` AS n WHERE q.`NCMCode` = n.`NCMCode` AND {_dateFilter} AND {filter} AND {_workcenterFilter} GROUP BY q.`NCMCode`";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sqlCmd, App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }
                        while (await reader.ReadAsync())
                        {
                            absoluteList.Add(new NCM { ChartCode = reader.GetString(3), Data = reader.GetInt32(1) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
            absoluteList = absoluteList.OrderByDescending(o => o.Data).ToList();
            var counter = 0;
            var _temp = 0;
            var cumulativeList = new List<int>();
            foreach (NCM n in absoluteList)
            {
                if (counter == 0)
                {
                    cumulativeList.Add((int)Math.Round(((double)n.Data / (double)absoluteList.Sum(o => o.Data)) * 100, 0));
                }
                else
                {
                    cumulativeList.Add((int)Math.Round(((double)n.Data / (double)absoluteList.Sum(o => o.Data)) * 100 + cumulativeList[counter - 1], 0));
                }
                if (cumulativeList[counter] >= percentage)
                {
                    break;
                }
                counter++;
            }
            counter++;
            if (counter < absoluteList.Count)
            {
                while (counter != absoluteList.Count)
                {
                    _temp += absoluteList[counter].Data;
                    absoluteList.RemoveAt(counter);
                }
                absoluteList.Add(new NCM { ChartCode = "All Others", Data = _temp });
                cumulativeList.Add(100);
            }
            counter = 0;
            foreach (NCM n in absoluteList)
            {
                _ncmCountData.Add(new QIRChart { NCMCode = n.ChartCode, Absolute = n.Data, Cumulative = cumulativeList[counter] });
                counter++;
            }
            return _ncmCountData;
        }

        /// <summary>
        /// Get QIR Numbers that match the selected data point in a QIR chart
        /// </summary>
        /// <param name="ncmSummary">NCM summary filter</param>
        /// <param name="workCenter">Origin work center filter</param>
        /// <param name="month">Filter month</param>
        /// <param name="year">Filter year</param>
        /// <returns>DataTable of QIRNumbers that match the given select filter</returns>
        public async static Task<DataTable> GetResultsAsync(string ncmSummary, int? workCenter, int month, int year)
        {
            var ncmCode = 0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand($"SELECT `NCMCode` FROM `{App.Schema}`.`ncm` WHERE `Summary`=@p1", App.ConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", ncmSummary);
                    ncmCode = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                }
                var adapterSelect = workCenter == 0
                    ? $"SELECT `QIRNumber` FROM `{App.Schema}`.`qir_metrics_view` WHERE `NCMCode`=@p1"
                    : $"SELECT `QIRNumber` FROM `{App.Schema}`.`qir_metrics_view` WHERE `NCMCode`=@p1 AND `Origin`=@p2";
                adapterSelect += month == 0
                     ? $" AND YEAR(`QIRDate`)=@p4"
                     : $" AND MONTH(`QIRDate`)=@p3 AND YEAR(`QIRDate`)=@p4";
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(adapterSelect, App.ConAsync))
                {
                    if (workCenter == 0)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", ncmCode);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p1", ncmCode);
                        adapter.SelectCommand.Parameters.AddWithValue("p2", workCenter);
                    }
                    if (month == 0)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p4", year);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("p3", month);
                        adapter.SelectCommand.Parameters.AddWithValue("p4", year);
                    }
                    var _table = new DataTable();
                    await adapter.FillAsync(_table).ConfigureAwait(false);
                    return _table;
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
