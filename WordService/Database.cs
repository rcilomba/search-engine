using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace WordService
{
    public class Database
    {
        private static Database _instance;
        private static readonly object _lock = new object();
        private Coordinator coordinator = new Coordinator();

        public static Database GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Database();
                    }
                }
            }
            return _instance;
        }

        private void Execute(IDbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            trans.Commit();
        }

        public void DeleteDatabase()
        {
            foreach (var connection in coordinator.GetAllConnections())
            {
                Execute(connection, "DROP TABLE IF EXISTS Occurrences");
                Execute(connection, "DROP TABLE IF EXISTS Words");
                Execute(connection, "DROP TABLE IF EXISTS Documents");
            }
        }

        public void RecreateDatabase()
        {
            Execute(coordinator.GetDocumentConnection(), "CREATE TABLE Documents(id INTEGER PRIMARY KEY, url VARCHAR(500))");
            Execute(coordinator.GetOccurrenceConnection(), "CREATE TABLE Occurrences(wordId INTEGER, docId INTEGER)");

            foreach (var connection in coordinator.GetAllWordConnections())
            {
                Execute(connection, "CREATE TABLE Words(id INTEGER PRIMARY KEY, name VARCHAR(500))");
            }
        }

        public Dictionary<int, int> GetDocuments(List<int> wordIds)
        {
            var res = new Dictionary<int, int>();
            var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) + " GROUP BY docId ORDER BY count DESC;";

            var connection = coordinator.GetOccurrenceConnection(); 
            var selectCmd = connection.CreateCommand();
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
            var res = new Dictionary<string, int>();

            foreach (var connection in coordinator.GetAllWordConnections())
            {
                var selectCmd = connection.CreateCommand();
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
            }
            return res;
        }

        public List<string> GetDocDetails(List<int> docIds)
        {
            var res = new List<string>();
            var connection = coordinator.GetDocumentConnection();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var url = reader.GetString(1);
                    res.Add(url);
                }
            }
            return res;
        }

        public void InsertAllWords(Dictionary<string, int> res)
        {
            foreach (var p in res)
            {
                using (var connection = coordinator.GetWordConnection(p.Key))
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = @"INSERT INTO Words(id, name) VALUES(@id,@name)";

                    command.Parameters.AddWithValue("@name", p.Key);
                    command.Parameters.AddWithValue("@id", p.Value);

                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }


        public void InsertAllOcc(int docId, ISet<int> wordIds)
        {
            var connection = coordinator.GetOccurrenceConnection(); 
            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO Occurrences(wordId, docId) VALUES(@wordId,@docId)";

                var paramwordId = command.CreateParameter();
                paramwordId.ParameterName = "wordId";
                command.Parameters.Add(paramwordId);

                var paramDocId = command.CreateParameter();
                paramDocId.ParameterName = "docId";
                paramDocId.Value = docId;
                command.Parameters.Add(paramDocId);

                foreach (var p in wordIds)
                {
                    paramwordId.Value = p;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void InsertDocument(int id, string url)
        {
            var connection = coordinator.GetDocumentConnection();  
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Documents(id, url) VALUES(@id,@url)";

            var pName = new SqlParameter("url", url);
            insertCmd.Parameters.Add(pName);

            var pCount = new SqlParameter("id", id);
            insertCmd.Parameters.Add(pCount);

            insertCmd.ExecuteNonQuery();
        }
    }
}
