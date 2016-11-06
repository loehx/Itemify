using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using NpgsqlTypes;

namespace Itemify.Core.PostgreSql.Util
{
    internal static class SqlUtil
    {
        public static string ToSql<T>(this T source)
        {
            if (source == null)
                return "NULL";

            var s = source as string;
            if (s != null)
                return ToSql((string)(object) source);

            return source.ToString();
        }

        public static string ToSql(this string source)
        {
            return '"' + source + '"';
        }

        public static string GetSqlTypeFromType(Type type)
        {
            if (type == typeof(string)) return "text";
            if (type == typeof(int)) return "int4";
            if (type == typeof(long)) return "int8";
            if (type == typeof(float)) return "float4";
            if (type == typeof(double)) return "float8";
            if (type == typeof(decimal)) return "numeric";
            if (type == typeof(DateTime)) return "timestamp";
            if (type == typeof(DateTimeOffset)) return "timestamptz";
            if (type == typeof(TimeSpan)) return "time";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "int2";
            if (type == typeof(sbyte)) return "int2";
            if (type == typeof(short)) return "int2";
            if (type == typeof(char[])) return "text";
            if (type == typeof(char)) return "text";
            if (type == typeof(NpgsqlPoint)) return "point";
            if (type == typeof(NpgsqlLSeg)) return "lseg";
            if (type == typeof(NpgsqlPath)) return "path";
            if (type == typeof(NpgsqlPolygon)) return "polygon";
            if (type == typeof(NpgsqlLine)) return "line";
            if (type == typeof(NpgsqlCircle)) return "circle";
            if (type == typeof(NpgsqlBox)) return "box";
            if (type == typeof(BitArray)) return "varbit";
            if (type == typeof(Guid)) return "uuid";
            if (type == typeof(IPAddress)) return "inet";
            if (type == typeof(NpgsqlInet)) return "inet";
            if (type == typeof(PhysicalAddress)) return "macaddr";
            if (type == typeof(NpgsqlTsQuery)) return "tsquery";
            if (type == typeof(NpgsqlTsVector)) return "tsvector";
            if (type == typeof(NpgsqlDate)) return "date";
            if (type == typeof(NpgsqlDateTime)) return "timestamp";
            if (type == typeof(NpgsqlDateTime)) return "timestamptz";
            if (type == typeof(byte[])) return "bytea";
            if (type == typeof(PostgisGeometry)) return "geometry";

            throw new Exception("Missing SQL type for .NET Type: " + type);
        }

    }
}
