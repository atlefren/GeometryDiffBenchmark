using NetTopologySuite.IO;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;

namespace BenchmarkFunctionApp
{
    internal class Row
    {
        public long Id { get; set; }
        public long InitialVersion { get; set; }
        public long LastVersion { get; set; }
    }

    public interface IGeometryReader
    {
        DiffData GetNext();
    }

    public class GeometryReader : IGeometryReader
    {
        private int _offset;
        private readonly string _versionTable;
        private readonly string _geomTable;
        private List<Row> _rows;

        public GeometryReader(string versionTable, string geomTable)
        {
            _offset = 0;
            _versionTable = versionTable;
            _geomTable = geomTable;
            _rows = new List<Row>();
        }

        private void GetRows()
        {
            var res = new List<Row>();
            using (var conn = GetConnection())
            {
                conn.Open();
                var query = string.Format(
                    $"SELECT id, initial_version, last_version FROM {_versionTable} LIMIT 10 OFFSET {_offset}");
                var command = new NpgsqlCommand(query, conn);
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        res.Add(new Row()
                        {
                            Id = dr.GetInt64(0),
                            InitialVersion = dr.GetInt64(1),
                            LastVersion = dr.GetInt64(1)
                        });
                    }
                }
            }

            _rows = res;
            _offset += 10;
        }

        private string GetGeom(long id, long version)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var query = string.Format(
                    $"SELECT ST_AsBinary(geom) FROM {_geomTable} WHERE id=:id AND version=:version");
                var command = new NpgsqlCommand(query, conn);
                command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Bigint) { Value = id });
                command.Parameters.Add(new NpgsqlParameter("version", NpgsqlDbType.Bigint) { Value = version });
                command.Prepare();
                using (var dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        return WKBWriter.ToHex((byte[])dr[0]);
                    }
                }
            }

            return null;
        }

        public DiffData GetNext()
        {
            if (_rows.Count == 0)
            {
                GetRows();
                if (_rows.Count == 0)
                {
                    return null;
                }
            }

            var row = _rows[0];
            _rows.RemoveAt(0);

            var initialGeom = GetGeom(row.Id, row.InitialVersion);
            var lastGeom = GetGeom(row.Id, row.LastVersion);
            if (initialGeom != null && lastGeom != null)
            {
                return new DiffData()
                {
                    GeometryId = $"{_geomTable}_{row.Id}",
                    OldGeom = initialGeom,
                    NewGeom = lastGeom
                };
            }

            return null;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection("Host=13.79.191.128;Username=atlefren;Password=nisse2gnom;Database=bench2");
        }
    }
}