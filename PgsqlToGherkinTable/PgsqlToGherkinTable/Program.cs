using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PgsqlToGherkinTable
{
    class Program
    {
        private static Dictionary<string, string> TableTypes;

        static void Main(string[] args)
        {
            // Check if database has the same one - we assume its allways en-US
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

            if (args.Length != 5)
            {
                Console.WriteLine($"<server> <database> <schema> <userid> <password>");
                Environment.Exit(1);
            }

            var server = args[0];
            var database = args[1];
            var tableSchema = args[2];
            var user = args[3];
            var password = args[4];

            using (var connection = new NpgsqlConnection($"Server={server};Database={database};User Id={user};Password={password};"))
            {
                Console.WriteLine($"Connecting to {server} ...");

                connection.Open();

                using (var streamWriter = new StreamWriter("result.txt"))
                {
                    Console.WriteLine($"Write result to {((FileStream)streamWriter.BaseStream).Name} ...");

                    InitializeTableTypes(connection, tableSchema);

                    WriteTableContentsToStream(tableSchema, connection, streamWriter);
                }
            }
        }

        private static void InitializeTableTypes(NpgsqlConnection connection, string schema)
        {
            TableTypes = connection.Query<KeyValuePair<string, string>>("SELECT CONCAT(table_schema, '.', table_name, '.', column_name) AS Key, data_type As Value FROM information_schema.columns WHERE table_schema = @schema", new { schema = schema }).ToDictionary(p => p.Key, p => p.Value);
        }

        private static void WriteTableContentsToStream(string tableSchema, NpgsqlConnection connection, StreamWriter writer)
        {
            string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = @schema AND table_name <> 'pg_stat_statements' ORDER BY table_name";
            
            foreach (var tableName in connection.Query<string>(query, new { schema = tableSchema }))
            {
                writer.WriteLine($"And table \"{tableSchema}\".\"{tableName}\" contains rows:");

                var result = connection.Query($"SELECT * FROM {tableSchema}.\"{tableName}\"");

                var rowsAsList = result.Cast<IDictionary<string, object>>().Select(r => ByPassTypeHandling(r, tableSchema, tableName));

                if (rowsAsList.Any())
                {
                    var length = new Dictionary<string, int>();

                    var headerBuilder = new StringBuilder("|");

                    foreach (var column in rowsAsList.First().Keys)
                    {
                        var maxContentLength = rowsAsList.SelectMany(r => r.Where(c => c.Key == column)).Max(r => r.Value.ToString().Length);
                        var maxLength = Math.Max(maxContentLength, column.Length);

                        length[column] = maxLength;

                        headerBuilder.AppendFormat($" {{0,-{maxLength}}} |", column);
                    }

                    writer.WriteLine(headerBuilder.ToString());

                    foreach (var rows in rowsAsList)
                    {
                        var rowBuilder = new StringBuilder("|");

                        foreach (var value in rows)
                        {
                            rowBuilder.AppendFormat($" {{0,-{length[value.Key]}}} |", value.Value);
                        }

                        writer.WriteLine(rowBuilder.ToString());
                    }
                }
                else
                {
                    var columns = connection.Query<string>("SELECT column_name FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table ORDER BY ordinal_position", new { schema = tableSchema, table = tableName });

                    var headerBuilder = new StringBuilder("|");

                    foreach (var column in columns)
                    {
                        headerBuilder.Append($" {column} |");
                    }

                    writer.WriteLine(headerBuilder.ToString());
                }

                writer.WriteLine();
            }
        }

        private static IDictionary<string, string> ByPassTypeHandling(IDictionary<string, object> rows, string tableSchema, string tableName)
        {
            var result = new Dictionary<string, string>();

            foreach (var column in rows)
            {
                var type = TableTypes[$"{tableSchema}.{tableName}.{column.Key}"];

                if (type == "timestamp without time zone" && column.Value != null)
                {
                    // ISO 8601
                    result[column.Key] = DateTime.Parse(column.Value.ToString()).ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else if (type == "timestamp with time zone" && column.Value != null)
                {
                    // ISO 8601
                    result[column.Key] = DateTime.Parse(column.Value.ToString()).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else
                {
                    result[column.Key] = column.Value == null ? string.Empty : column.Value.ToString();
                }
            }

            return result;
        }
    }
}
