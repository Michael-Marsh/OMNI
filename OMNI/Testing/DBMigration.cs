using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace OMNI.Testing
{
    public class DBMigration
    {
        public static void RunMe()
        {
            {
                var _tempList = new Dictionary<int, DateTime>();
                using (MySqlCommand cmd = new MySqlCommand("SELECT `QIRNumber`, `revision_date` FROM omni.qir_revisions WHERE `QIRNumber`>1004053", App.ConAsync))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!_tempList.ContainsKey(reader.GetInt32(0)))
                            {
                                _tempList.Add(reader.GetInt32(0), reader.GetDateTime(1));
                            }
                            else
                            {
                                _tempList.Remove(reader.GetInt32(0));
                                _tempList.Add(reader.GetInt32(0), reader.GetDateTime(1));
                            }
                        }
                    }
                }
                foreach (var item in _tempList)
                {
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE omni.qir_master SET `QIRDate`=@p1 WHERE `QIRNumber`=@p0", App.ConAsync))
                    {
                        cmd.Parameters.AddWithValue("p1", item.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("p0", item.Key);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
