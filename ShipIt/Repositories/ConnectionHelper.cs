using System.Configuration;

namespace ShipIt.Repositories
{
    public class ConnectionHelper
    {
        public static string GetConnectionString()
        {
            string dbname = ConfigurationManager.AppSettings["RDS_DB_NAME"];

            if (dbname == null)
            {
                return System.Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
            };

            string username = ConfigurationManager.AppSettings["RDS_USERNAME"];
            string password = ConfigurationManager.AppSettings["RDS_PASSWORD"];
            string hostname = ConfigurationManager.AppSettings["RDS_HOSTNAME"];
            string port = ConfigurationManager.AppSettings["RDS_PORT"];

            return "Server=" + hostname + ";Port=" + port + ";Database=" + dbname + ";User ID=" + username + ";Password=" + password + ";";
        }
    }
}
