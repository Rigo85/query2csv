using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Query2CSV
{
    class Executer
    {
        private string report = string.Empty;
        private string connectionString;

        public Executer(List<string> filePaths, string connectionString)
        {
            this.connectionString = connectionString;

            filePaths
                .Select(ExecuteQuery)
                .ToList()
                .ForEach(Save2File);
        }

        private void Save2File(Tuple<DataSet, string> tuple)
        {
            var ds = tuple.Item1;
            var filePath = tuple.Item2;

            var rows = ds.Tables[0].Rows;
            var cols = ds.Tables[0].Columns;

            var headers = string.Join(";", Enumerable.Range(0, cols.Count).Select(i => cols[i].ColumnName));

            var data = string.Join("\r\n", Enumerable
                .Range(0, rows.Count)
                .Select(i => Row2String(rows[i])));

            var output = $"{headers}\r\n{data}";

            File.WriteAllText(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    $"PDP-{Path.GetFileNameWithoutExtension(filePath)}-{DateTime.Now:dd-MM-yyyy-hh-mm-ss}.csv"),
                output,
                Encoding.Default);
        }

        private string Row2String(DataRow dataRow)
        {

            return string.Join(";", dataRow.ItemArray);
        }

        private Tuple<DataSet, string> ExecuteQuery(string path)
        {
            var ds = new DataSet();

            var queryBody = File.ReadAllText(path, Encoding.ASCII);

            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(queryBody, conn))
                {
                    cmd.CommandTimeout = 120;
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }

            return Tuple.Create(ds, path);
        }

    }
}
