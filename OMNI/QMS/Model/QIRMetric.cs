using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.Models;
using OMNI.QMS.Enumeration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace OMNI.QMS.Model
{
    public class QIRMetric
    {
        #region Properties

        public int InternalCountYTD { get; set; }
        public double InternalCostYTD { get; set; }
        public string InternalPercentOfSalesYTD { get; set; }
        public int IncomingCountYTD { get; set; }
        public double IncomingCostYTD { get; set; }
        public string IncomingPercentOfSalesYTD { get; set; }

        public int InternalCountMTD { get; set; }
        public double InternalCostMTD { get; set; }
        public string InternalPercentOfSalesMTD { get; set; }
        public int IncomingCountMTD { get; set; }
        public double IncomingCostMTD { get; set; }
        public string IncomingPercentOfSalesMTD { get; set; }

        #endregion

        /// <summary>
        /// QIRMetric Object Constructor
        /// </summary>
        public QIRMetric(int monthlySales)
        {
            try
            {
                var _yearlySales = OMNIDataBase.YearlySalesAsync(DateTime.Today.Year).Result + monthlySales;
                using (DataTable dt = new DataTable())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                                        SELECT [TotalCost], [SupplierID], [QIRDate] FROM [qir_metrics_view] WHERE [QIRDate] >= '{DateTime.Today.Year}-01-01'", App.SqlConAsync))
                    {
                        adapter.Fill(dt);
                    }
                    InternalCountYTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") == 0);
                    InternalCostYTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID = 0"));
                    InternalPercentOfSalesYTD = _yearlySales != 0 ? (InternalCostYTD / _yearlySales).ToString("P3") : "No Sales";
                    IncomingCountYTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") > 0);
                    IncomingCostYTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID > 0"));
                    IncomingPercentOfSalesYTD = _yearlySales != 0 ? (IncomingCostYTD / _yearlySales).ToString("P3") : "No Sales";
                    var _filter = $"SupplierID = 0 AND QIRDate >= #{DateTime.Today.Month}/01/{DateTime.Today.Year}# AND QIRDate <= #{DateTime.Today.Month}/{DateTime.Today.LastDayOfMonth()}/{DateTime.Today.Year}#";
                    InternalCountMTD = Convert.ToInt32(dt.Compute("COUNT(SupplierID)", _filter));
                    InternalCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", _filter));
                    InternalPercentOfSalesMTD = monthlySales != 0 ? (InternalCostMTD / monthlySales).ToString("P3") : "No Sales";
                    _filter = $"SupplierID > 0 AND QIRDate >= #{DateTime.Today.Month}/01/{DateTime.Today.Year}# AND QIRDate <= #{DateTime.Today.Month}/{DateTime.Today.LastDayOfMonth()}/{DateTime.Today.Year}#";
                    IncomingCountMTD = Convert.ToInt32(dt.Compute("COUNT(SupplierID)", _filter));
                    try
                    {
                        IncomingCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", _filter));
                    }
                    catch (Exception)
                    {
                        IncomingCostMTD = 0;
                    }
                    IncomingPercentOfSalesMTD = monthlySales != 0 ? (IncomingCostMTD / monthlySales).ToString("P3") : "No Sales";
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    public static class QIRMetricExtensions
    {
        /// <summary>
        /// Update the Internal QIR Metrics based of a selected month and year
        /// </summary>
        /// <param name="metric">Current QIRMetric object</param>
        /// <param name="month">Selected internal month as string("MMMM")</param>
        /// <param name="year">Selected internal year</param>
        /// <param name="mType">Metric Type to update</param>
        public static void UpdateMTDMetrics(this QIRMetric metric, string month, int year, QMSMetricType mType)
        {
            if (!string.IsNullOrEmpty(month) && year != 0 && metric != null)
            {
                var _month = Convert.ToDateTime($"01/{month}/2000").Month;
                var _monthlySales = OMNIDataBase.MonthlySalesAsync(month, year).Result[0];
                if (_monthlySales == 0)
                {
                    _monthlySales = M2k.GetLiveSales($"{DateTime.Today.Month}-1-{DateTime.Today.Year}", DateTime.Today.ToString("MM-dd-yyyy"));
                }
                using (DataTable dt = new DataTable())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                                        SELECT
                                                                            [TotalCost], [SupplierID]
                                                                        FROM
                                                                            [qir_metrics_view]
                                                                        WHERE
                                                                            [QIRDate] BETWEEN '{year}-{_month}-01' AND '{year}-{_month}-{Convert.ToDateTime($"01/{_month}/{year}").LastDayOfMonth()}'", App.SqlConAsync))
                    {
                        adapter.Fill(dt);
                    }
                    switch (mType)
                    {
                        case QMSMetricType.Internal:
                            metric.InternalCountMTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") == 0);
                            metric.InternalCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID = 0"));
                            metric.InternalPercentOfSalesMTD = _monthlySales != 0 ? (metric.InternalCostMTD / _monthlySales).ToString("P3") : "No Sales";
                            break;
                        case QMSMetricType.Incoming:
                            metric.IncomingCountMTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") > 0);
                            metric.IncomingCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID > 0"));
                            metric.IncomingPercentOfSalesMTD = _monthlySales != 0 ? (metric.IncomingCostMTD / _monthlySales).ToString("P3") : "No Sales";
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Update all the values in the QIR Metrics
        /// </summary>
        /// <param name="metric">Current QIRMetric object</param>
        /// <param name="monthlySales">Live Monthly Sales</param>
        public static void UpdateAllMetrics(this QIRMetric metric, int monthlySales)
        {
            try
            {
                var _yearlySales = OMNIDataBase.YearlySalesAsync(DateTime.Today.Year).Result + monthlySales;
                using (DataTable dt = new DataTable())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter($@"USE {App.DataBase};
                                                                        SELECT [TotalCost], [SupplierID], [QIRDate] FROM [qir_metrics_view] WHERE [QIRDate] >= '{DateTime.Today.Year}-01-01'", App.SqlConAsync))
                    {
                        adapter.Fill(dt);
                    }
                    metric.InternalCountYTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") == 0);
                    metric.InternalCostYTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID = 0"));
                    metric.InternalPercentOfSalesYTD = _yearlySales != 0 ? (metric.InternalCostYTD / _yearlySales).ToString("P3") : "No Sales";
                    metric.IncomingCountYTD = dt.AsEnumerable().Count(r => r.Field<int>("SupplierID") > 0);
                    metric.IncomingCostYTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", "SupplierID > 0"));
                    metric.IncomingPercentOfSalesYTD = _yearlySales != 0 ? (metric.IncomingCostYTD / _yearlySales).ToString("P3") : "No Sales";
                    var _filter = $"SupplierID = 0 AND QIRDate >= #{DateTime.Today.Month}/01/{DateTime.Today.Year}# AND QIRDate <= #{DateTime.Today.Month}/{DateTime.Today.LastDayOfMonth()}/{DateTime.Today.Year}#";
                    metric.InternalCountMTD = Convert.ToInt32(dt.Compute("COUNT(SupplierID)", _filter));
                    metric.InternalCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", _filter));
                    metric.InternalPercentOfSalesMTD = monthlySales != 0 ? (metric.InternalCostMTD / monthlySales).ToString("P3") : "No Sales";
                    _filter = $"SupplierID > 0 AND QIRDate >= #{DateTime.Today.Month}/01/{DateTime.Today.Year}# AND QIRDate <= #{DateTime.Today.Month}/{DateTime.Today.LastDayOfMonth()}/{DateTime.Today.Year}#";
                    metric.IncomingCountMTD = Convert.ToInt32(dt.Compute("COUNT(SupplierID)", _filter));
                    try
                    {
                        metric.IncomingCostMTD = Convert.ToDouble(dt.Compute("SUM(TotalCost)", _filter));
                    }
                    catch (Exception)
                    {
                        metric.IncomingCostMTD = 0;
                    }
                    metric.IncomingPercentOfSalesMTD = monthlySales != 0 ? (metric.IncomingCostMTD / monthlySales).ToString("P3") : "No Sales";
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
