using OMNI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OMNI.Extensions
{
    /// <summary>
    /// DataTable Extensions Interaction Logic
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Search a DataTable for a value
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        public static void Search(this DataTable table, string query)
        {
            var columnNames = (from dc in table?.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
            var counter = 0;
            var queryBuilder = new StringBuilder();
            foreach (var name in columnNames)
            {
                if (counter == 0)
                {
                    queryBuilder.Append($"Convert({name}, 'System.String') LIKE '%{query}%'");
                }
                else
                {
                    queryBuilder.Append($"OR Convert({name}, 'System.String') LIKE '%{query}%'");
                }
                counter++;
            }
            table.DefaultView.RowFilter = queryBuilder.ToString();
        }

        /// <summary>
        /// Search a DataTable for a value
        /// </summary>
        /// <param name="table">Source DataTable</param>
        /// <param name="query">Search string</param>
        /// <returns>Filtered DataView</returns>
        public static DataView SearchToDataView(this DataTable table, string query)
        {
            try
            {
                var columnNames = (from dc in table.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
                var counter = 0;
                var queryBuilder = new StringBuilder();
                foreach (var name in columnNames)
                {
                    if (counter == 0)
                    {
                        queryBuilder.Append($"Convert({name}, 'System.String') LIKE '%{query}%'");
                    }
                    else
                    {
                        queryBuilder.Append($"OR Convert({name}, 'System.String') LIKE '%{query}%'");
                    }
                    counter++;
                }
                return new DataView(table) { RowFilter = queryBuilder.ToString() };
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// DataTable extension for deleteing rows
        /// </summary>
        /// <param name="table">DataTable</param>
        /// <param name="filter">Filter expression</param>
        /// <returns>Altered DataTable</returns>
        public static DataTable Delete(this DataTable table, string filter)
        {
            table.Select(filter).Delete();
            return table;
        }

        /// <summary>
        /// Delete function for DataTable
        /// </summary>
        /// <param name="rows">DataRows in DataTable</param>
        public static void Delete(this IEnumerable<DataRow> rows)
        {
            foreach (var row in rows)
                row.Delete();
        }

        /// <summary>
        /// Custom sorting
        /// </summary>
        /// <param name="table">DataTable</param>
        /// <param name="comp">Data row comparison</param>
        public static void Sort(this DataTable table, Comparison<DataRow> comp)
        {
            var _rowList = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                _rowList.Add(row);
            }
            _rowList.Sort(comp);
            table.Clear();
            table.AcceptChanges();
            foreach (DataRow row in _rowList)
            {
                table.Rows.Add(row.ItemArray);
            }
        }
    }
}
