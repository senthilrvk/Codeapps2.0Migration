using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace CodeAppsDataMigration.Data
{
    public static class SqlServerConnection
    {
        public static string GetConnectionString()
        {
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionStrings.xml");
            var doc = XDocument.Load(xmlPath);
            var sql = doc.Root!.Element("SqlServer")!;

            return
                $"Server={sql.Element("Server")!.Value};" +
                $"Database={sql.Element("Database")!.Value};" +
                $"User Id={sql.Element("UserId")!.Value};" +
                $"Password={sql.Element("Password")!.Value};" +
                $"TrustServerCertificate={sql.Element("TrustServerCertificate")!.Value};" +
                $"Connection Timeout={sql.Element("ConnectionTimeout")!.Value};" +
                $"MultipleActiveResultSets={sql.Element("MultipleActiveResultSets")!.Value};" +
                $"Encrypt={sql.Element("Encrypt")!.Value};";
        }

        public static SqlConnection Create()
        {
            return new SqlConnection(GetConnectionString());
        }
    }
}
