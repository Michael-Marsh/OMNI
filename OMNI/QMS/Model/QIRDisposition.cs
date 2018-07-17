using OMNI.Extensions;
using OMNI.Helpers;
using OMNI.QMS.Enumeration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OMNI.QMS.Model
{
    public class QIRDisposition
    {
        #region Properties

        public string Description { get; set; }
        public QIRStatus Status { get; set; }

        #endregion

        /// <summary>
        /// List of QIR Dispositions
        /// </summary>
        /// <returns>Generated List of QIR Dispositions</returns>
        public async static Task<List<QIRDisposition>> GetQIRDispositionListAsync()
        {
            var _qirDispositionList = new List<QIRDisposition>();
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [qir_disposition]", App.SqlConAsync))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            _qirDispositionList.Add(new QIRDisposition { Description = reader.SafeGetString(nameof(Description)), Status = (QIRStatus)Enum.Parse(typeof(QIRStatus),reader.SafeGetString(nameof(Status))) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
            }
            return _qirDispositionList;
        }

        /// <summary>
        /// Get a QIR Dispostion Object
        /// </summary>
        /// <param name="description">QIR Disposition Description</param>
        /// <returns>Formated QIR Disposition Object</returns>
        public async static Task<QIRDisposition> GetQIRDispositionAsync(string description)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand($"USE {App.DataBase}; SELECT * FROM [qir_disposition] WHERE [Description]=@p1", App.SqlConAsync))
                {
                    cmd.Parameters.AddWithValue("p1", description);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            return new QIRDisposition { Description = reader.SafeGetString(nameof(Description)), Status = (QIRStatus)Enum.Parse(typeof(QIRStatus), reader.SafeGetString(nameof(Status))) };
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                return null;
            }
        }
    }
}
