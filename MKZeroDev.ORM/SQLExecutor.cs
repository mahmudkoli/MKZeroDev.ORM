using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;

namespace MKZeroDev.ORM
{
    public class SQLExecutor : IDisposable
    {
        private readonly string _connectionString = default!;
        private SqlConnection _connection = default!;
        private SqlCommand _command = default!;
        private bool _isConnectionOpen = default!;

        public SQLExecutor(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("Connection string is empty or null");
            _connectionString = connectionString;
            EnsureConnection();
            EnsureCommand();
        }

        #region Ensure Connection and Command
        private void EnsureConnection()
        {
            try
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                _isConnectionOpen = true;
            }
            catch (SqlException ex)
            {
                throw new Exception("SQL connection couldn't be established", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void EnsureCommand()
        {
            try
            {
                var query = "SELECT 1";
                _command = new SqlCommand(query, _connection);
                _command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw new Exception("SQL command couldn't be executed", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Query
        public IList<dynamic> GetDynamic(string sql, Dictionary<string, object> parameters)
        {
            var items = new List<dynamic>();

            _command.CommandText = sql;
            _command.Parameters.Clear();
            _command.CommandType = CommandType.StoredProcedure;
            if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

            foreach (var param in parameters)
            {
                DbParameter dbParameter = _command.CreateParameter();
                dbParameter.ParameterName = param.Key;
                dbParameter.Value = param.Value;
                _command.Parameters.Add(dbParameter);
            }

            using (var reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = new ExpandoObject() as IDictionary<string, object>;
                    for (var count = 0; count < reader.FieldCount; count++)
                    {
                        item.Add(reader.GetName(count), reader[count]);
                    }
                    items.Add(item);
                }
            }

            return items;
        }

        public DataSet GetDataSet(string sql, Dictionary<string, object> parameters, bool isStoredProcedure = false)
        {
            var dataSet = new DataSet();
            DbProviderFactory dbFactory = DbProviderFactories.GetFactory(_connection);

            _command.CommandText = sql;
            _command.Parameters.Clear();
            if (isStoredProcedure) { _command.CommandType = CommandType.StoredProcedure; }
            if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

            foreach (var param in parameters)
            {
                DbParameter dbParameter = _command.CreateParameter();
                dbParameter.ParameterName = param.Key;
                dbParameter.Value = param.Value;
                _command.Parameters.Add(dbParameter);
            }

            using (var adapter = dbFactory.CreateDataAdapter())
            {
                adapter.SelectCommand = _command;
                adapter.Fill(dataSet);
            }

            return dataSet;
        }

        public IList<T> GetAll<T>(string sql, Dictionary<string, object>? parameters = null, bool isStoredProcedure = false)
        {
            var items = new List<T>();

            _command.CommandText = sql;
            _command.Parameters.Clear();
            if (isStoredProcedure) { _command.CommandType = CommandType.StoredProcedure; }
            if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    DbParameter dbParameter = _command.CreateParameter();
                    dbParameter.ParameterName = param.Key;
                    dbParameter.Value = param.Value;
                    _command.Parameters.Add(dbParameter);
                }
            }

            using (var reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = typeof(T);
                    var instance = ObjectReflector.CreateInstance(type, new object[] { });
                    var properties = type.GetProperties();

                    foreach (var property in properties)
                    {
                        if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                        var dbColDef = ObjectExtensions.GetDbColumnDef(property);

                        if (!reader.IsDBNull(dbColDef.ColumnName))
                            property.SetValue(instance, reader[dbColDef.ColumnName]);
                    }

                    items.Add((T)instance);
                }
            }

            return items;
        }

        public T? GetFirstOrDefault<T>(string sql, Dictionary<string, object>? parameters = null, bool isStoredProcedure = false)
        {
            return GetAll<T>(sql, parameters, isStoredProcedure).FirstOrDefault();
        }

        public (IList<T> Items, int Total, int TotalFilter) Get<T>(string sql, IList<(string Key, object Value, bool IsOut)> parameters, bool isStoredProcedure = false)
        {
            var items = new List<T>();
            int? totalCount = 0;
            int? filteredCount = 0;

            _command.CommandText = sql;
            _command.Parameters.Clear();
            if (isStoredProcedure) { _command.CommandType = CommandType.StoredProcedure; }
            if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

            foreach (var param in parameters)
            {
                DbParameter dbParameter = _command.CreateParameter();
                dbParameter.ParameterName = param.Key;
                if (!param.IsOut)
                {
                    dbParameter.Value = param.Value;
                }
                else
                {
                    dbParameter.Direction = ParameterDirection.Output;
                    dbParameter.DbType = DbType.Int32;
                }
                _command.Parameters.Add(dbParameter);
            }

            using (var reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var type = typeof(T);
                    var instance = ObjectReflector.CreateInstance(type, new object[] { });
                    var properties = type.GetProperties();

                    foreach (var property in properties)
                    {
                        if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                        var dbColDef = ObjectExtensions.GetDbColumnDef(property);

                        if (!reader.IsDBNull(dbColDef.ColumnName))
                            property.SetValue(instance, reader[dbColDef.ColumnName]);
                    }

                    items.Add((T)instance);
                }
            }

            totalCount = (int?)_command.Parameters["TotalCount"].Value;
            filteredCount = (int?)_command.Parameters["FilteredCount"].Value;

            return (items, totalCount ?? 0, filteredCount ?? 0);
        }

        public (IList<dynamic> Items, int Total, int TotalFilter) Get(string sql, IList<(string Key, object Value, bool IsOut)> parameters)
        {
            var items = new List<dynamic>();
            int? totalCount = 0;
            int? filteredCount = 0;

            _command.CommandText = sql;
            _command.Parameters.Clear();
            _command.CommandType = CommandType.StoredProcedure;
            if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

            foreach (var param in parameters)
            {
                DbParameter dbParameter = _command.CreateParameter();
                dbParameter.ParameterName = param.Key;
                if (!param.IsOut)
                {
                    dbParameter.Value = param.Value;
                }
                else
                {
                    dbParameter.Direction = ParameterDirection.Output;
                    dbParameter.DbType = DbType.Int32;
                }
                _command.Parameters.Add(dbParameter);
            }

            using (var reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = new ExpandoObject() as IDictionary<string, object>;
                    for (var count = 0; count < reader.FieldCount; count++)
                    {
                        item.Add(reader.GetName(count), reader[count]);
                    }
                    items.Add(item);
                }
            }

            totalCount = (int?)_command.Parameters["TotalCount"].Value;
            filteredCount = (int?)_command.Parameters["FilteredCount"].Value;

            return (items, totalCount ?? 0, filteredCount ?? 0);
        }
        #endregion

        #region Command
        public int ExecuteCommand(string sql, Dictionary<string, object?>? parameters = null,bool isStoredProcedure = false)
        {
            try
            {
                _command.CommandText = sql;
                _command.Parameters.Clear();
                if (isStoredProcedure) { _command.CommandType = CommandType.StoredProcedure; }
                if (_command.Connection.State != ConnectionState.Open) { _command.Connection.Open(); }

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        DbParameter dbParameter = _command.CreateParameter();
                        dbParameter.ParameterName = param.Key;
                        dbParameter.Value = param.Value ?? DBNull.Value;
                        _command.Parameters.Add(dbParameter);
                    }
                }

                var rowAffected = _command.ExecuteNonQuery();

                return rowAffected;
            }
            catch (SqlException ex)
            {
                throw new Exception("Failed to execute command", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to execute command", ex);
            }
        }
        #endregion

        #region Helper
        public bool IsExistsDatabase(string databaseName)
        {
            try
            {
                var connSB = new SqlConnectionStringBuilder(_connectionString);
                connSB.InitialCatalog = string.Empty;

                using (SqlConnection conn = new SqlConnection(connSB.ConnectionString))
                {
                    conn.Open();
                    var query = $"SELECT NAME FROM SYS.DATABASES WHERE NAME = '{databaseName}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var res = cmd.ExecuteScalar();

                        return res != null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("SQL database couldn't be found", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool IsExistsTable(string tableName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var query = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var res = cmd.ExecuteScalar();

                        return res != null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("SQL table couldn't be found", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Dispose
        ~SQLExecutor()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_isConnectionOpen)
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
                _isConnectionOpen = false;
            }

            if (_command != null)
            {
                _command.Dispose();
            }
        }
        #endregion
    }
}
