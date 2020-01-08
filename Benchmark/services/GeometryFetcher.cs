using GeoAPI.Geometries;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.IO;
using Npgsql;
using NpgsqlTypes;

namespace Benchmark.services
{
    public interface IGeometryFetcher
    {
        IGeometry GetGeom(string table, long id, long version);
    }

    public class GeometryFetcher : IGeometryFetcher
    {
        private readonly string _connStr;

        public GeometryFetcher(IConfiguration configuration)
        {
            _connStr = configuration.GetValue<string>("DbConnStr");
        }

        private readonly WKBReader _wkbReader = new WKBReader();

        public IGeometry GetGeom(string table, long id, long version)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var query = string.Format(
                    $"SELECT ST_AsBinary(geom) FROM {table} WHERE id=:id AND version=:version");
                var command = new NpgsqlCommand(query, conn);
                command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id });
                command.Parameters.Add(new NpgsqlParameter("version", NpgsqlDbType.Bigint) { Value = version });
                command.Prepare();
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        return _wkbReader.Read((byte[])dr[0]);
                    }
                }
            }

            return null;
        }

        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connStr);
        }
    }
}