using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace ConsoleSearch
{
    public class Database
    {
        private SqlConnection _connection;
        public Database()
        {
            _connection = new ("Server=localhost;User Id=sa;Password=SuperSecret7!;Encrypt=false;");
            _connection.Open();
        }

        private void Execute(string sql)
        {
            using var trans = _connection.BeginTransaction();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            trans.Commit();
        }

        // key is the id of the document, the value is number of search words in the document
        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            var res = new Dictionary<int, int>();

            var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) + " GROUP BY docId ORDER BY count DESC;";

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = sql;

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var docId = reader.GetInt32(0);
                    var count = reader.GetInt32(1);
                   
                    res.Add(docId, count);
                }
            }

            return res;
        }

        private string AsString(List<int> x)
        {
            return string.Concat("(", string.Join(',', x.Select(i => i.ToString())), ")");
        }

        public Dictionary<string, int> GetAllWords()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();
      
            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Words";

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var w = reader.GetString(1);
                    
                    res.Add(w, id);
                }
            }
            return res;
        }

        public List<string> GetDocDetails(List<int> docIds)
        {
            List<string> res = new List<string>();

            var selectCmd = _connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var url = reader.GetString(1);

                    res.Add(url);
                }
            }
            return res;
        }
    }
}
