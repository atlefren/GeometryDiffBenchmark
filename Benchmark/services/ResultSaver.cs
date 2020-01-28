using Benchmark.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;

namespace Benchmark.services
{
    public interface IResultSaver
    {
        void SaveResult(string table, Dictionary<string, DiffResult> result, long id);
    }

    public class ResultSaver : IResultSaver
    {
        private readonly string _connStr;

        public ResultSaver(IConfiguration configuration)
        {
            _connStr = configuration.GetValue<string>("DbConnStr");
        }


        public void SaveResult(string table, Dictionary<string, DiffResult> result, long id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                conn.TypeMapper.UseJsonNet();
                using (var cmd = new NpgsqlCommand(
                    $"INSERT INTO {table} (id, textDiffer, jsonDiffer, binaryDiffer, geomDiffer) VALUES(@id, @text, @json, @binary, @geom)",
                    conn))
                {
                    cmd.Parameters.AddWithValue("id", id);

                    cmd.Parameters.Add(new NpgsqlParameter("text", NpgsqlDbType.Jsonb) { Value = result["TextDiffer"] });
                    cmd.Parameters.Add(new NpgsqlParameter("json", NpgsqlDbType.Jsonb) { Value = result["JsonDiffer"] });
                    cmd.Parameters.Add(new NpgsqlParameter("binary", NpgsqlDbType.Jsonb)
                    { Value = result["BinaryDiffer"] });
                    cmd.Parameters.Add(new NpgsqlParameter("geom", NpgsqlDbType.Jsonb)
                    { Value = result["GeomDiffer"] });


                    cmd.ExecuteNonQuery();
                }
            }
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connStr);
        }
    }
}