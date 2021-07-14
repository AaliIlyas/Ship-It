using Npgsql;
using ShipIt.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ShipIt.Repositories
{
    public abstract class RepositoryBase
    {
        private IDbConnection Connection => new NpgsqlConnection(ConnectionHelper.GetConnectionString());

        protected long QueryForLong(string sqlString)
        {
            using (IDbConnection connection = Connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sqlString;
                connection.Open();
                IDataReader reader = command.ExecuteReader();

                try
                {
                    reader.Read();
                    return reader.GetInt64(0);
                }
                finally
                {
                    reader.Close();
                }
            };
        }

        protected void RunSingleQuery(string sql, string noResultsExceptionMessage, params NpgsqlParameter[] parameters)
        {
            using (IDbConnection connection = Connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                foreach (NpgsqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                connection.Open();
                IDataReader reader = command.ExecuteReader();

                try
                {
                    if (reader.RecordsAffected != 1)
                    {
                        throw new NoSuchEntityException(noResultsExceptionMessage);
                    }
                    reader.Read();
                }
                finally
                {
                    reader.Close();
                }
            };
        }

        protected int RunSingleQueryAndReturnRecordsAffected(string sql, params NpgsqlParameter[] parameters)
        {
            using (IDbConnection connection = Connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                foreach (NpgsqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
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
                return reader.RecordsAffected;
            };
        }

        protected TDataModel RunSingleGetQuery<TDataModel>(string sql, Func<IDataReader, TDataModel> mapToDataModel, string noResultsExceptionMessage, params NpgsqlParameter[] parameters)
        {
            return RunGetQuery(sql, mapToDataModel, noResultsExceptionMessage, parameters).Single();
        }

        protected IEnumerable<TDataModel> RunGetQuery<TDataModel>(string sql, Func<IDataReader, TDataModel> mapToDataModel, string noResultsExceptionMessage, params NpgsqlParameter[] parameters)
        {
            using (IDbConnection connection = Connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (NpgsqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
                connection.Open();
                IDataReader reader = command.ExecuteReader();

                try
                {
                    if (!reader.Read())
                    {
                        throw new NoSuchEntityException(noResultsExceptionMessage);
                    }
                    yield return mapToDataModel(reader);

                    while (reader.Read())
                    {
                        yield return mapToDataModel(reader);
                    }
                }
                finally
                {
                    reader.Close();
                }
            };
        }

        protected void RunQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using (IDbConnection connection = Connection)
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                foreach (NpgsqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
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
            };
        }

        protected void RunTransaction(string sql, List<NpgsqlParameter[]> parametersList)
        {
            using (IDbConnection connection = Connection)
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                IDbTransaction transaction = connection.BeginTransaction();
                command.Transaction = transaction;
                List<int> recordsAffected = new List<int>();

                try
                {
                    foreach (NpgsqlParameter[] parameters in parametersList)
                    {
                        command.CommandText = sql;
                        command.Parameters.Clear();

                        foreach (NpgsqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }

                        recordsAffected.Add(command.ExecuteNonQuery());
                    }

                    for (int i = 0; i < recordsAffected.Count; i++)
                    {
                        if (recordsAffected[i] == 0)
                        {
                            throw new Exception();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}