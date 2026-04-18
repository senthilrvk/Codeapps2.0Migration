using System.Xml.Linq;
using Npgsql;

namespace CodeAppsDataMigration.Data
{
    public static class PostgresConnection
    {
        public static string GetConnectionString()
        {
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
            var doc = XDocument.Load(xmlPath);
            var pg = doc.Root!.Element("Postgres")!;

            return
                $"Host={pg.Element("Host")!.Value};" +
                $"Port={pg.Element("Port")!.Value};" +
                $"Database={pg.Element("Database")!.Value};" +
                $"Username={pg.Element("Username")!.Value};" +
                $"Password={pg.Element("Password")!.Value};" +
                $"Timeout={pg.Element("Timeout")!.Value};" +
                $"CommandTimeout={pg.Element("CommandTimeout")!.Value};" +
                $"KeepAlive={pg.Element("KeepAlive")!.Value};" +
                $"Pooling={pg.Element("Pooling")!.Value};" +
                $"Maximum Pool Size={pg.Element("MaxPoolSize")!.Value};" +
                $"Include Error Detail={pg.Element("IncludeErrorDetail")!.Value};";
        }

        public static NpgsqlConnection Create()
        {
            return new NpgsqlConnection(GetConnectionString());
        }
    }
}
