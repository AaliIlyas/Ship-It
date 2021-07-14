using Npgsql;
using ShipIt.Repositories;
using System.Data;

namespace ShipItTest
{
    public abstract class AbstractBaseTest
    {

        protected EmployeeRepository EmployeeRepository { get; set; }
        protected ProductRepository ProductRepository { get; set; }
        protected CompanyRepository CompanyRepository { get; set; }
        protected StockRepository StockRepository { get; set; }

        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(System.Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") ?? "");
        }

        public void onSetUp()
        {
            DotNetEnv.Env.Load();
            // Start from a clean slate
            string sql =
                "TRUNCATE TABLE em;"
                + "TRUNCATE TABLE stock;"
                + "TRUNCATE TABLE gcp;"
                + "TRUNCATE TABLE gtin CASCADE;";

            using (IDbConnection connection = CreateSqlConnection())
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                IDataReader reader = command.ExecuteReader();
                try
                {
                    reader.Read();
                }
                finally
                {
                    reader.Close();
                }
            }

        }
    }
}



