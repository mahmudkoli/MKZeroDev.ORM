﻿using System.Data.SqlClient;

namespace MKZeroDev.ORM
{
    public class ORMInitializer
    {
        private readonly string _connectionString = default!;

        public ORMInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DatabaseInitialize<T>() where T : ORMDatabase
        {
            // database create if not exists
            CreateDatabaseIfNotExists();

            var ormExecutor = new ORMExecutor(_connectionString);

            // ORM information table create if not exists
            ormExecutor.CreateTableIfNotExists<ORMInformation>();

            // existing schemas from database
            var existingSchemas = ormExecutor.GetExistingDBSchemas();

            // current schemas
            var contextType = typeof(T);
            var currentSchemaTypes = contextType.GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(ORMTable<>)).Select(x => x.PropertyType).ToList();
            
            var currentSchemas = new Dictionary<string, Type>();
            foreach (var currentSchemaType in currentSchemaTypes)
            {
                var type = currentSchemaType.GenericTypeArguments[0];
                var obj = ObjectReflector.CreateInstance(type, new object[] { });
                var tableName = (string?)ObjectReflector.CallGenericStaticMethod(typeof(ObjectExtensions), nameof(ObjectExtensions.GetDbTableName), new Type[] { type }, new Type[] { }, new object[] { });
                currentSchemas.Add(tableName, type);
            }

            // create or modify schemas
            foreach (var currentSchema in currentSchemas)
            {
                var isExistsInDB = existingSchemas.Any(x => x.TableName == currentSchema.Key);

                if (!isExistsInDB)
                {
                    var res = (int?)ObjectReflector.CallGenericMethod(ormExecutor.GetType(), ormExecutor, nameof(ORMExecutor.CreateTable), new Type[] { currentSchema.Value }, new Type[] { }, new object[] { });
                }
                else
                {
                    var res = (int?)ObjectReflector.CallGenericMethod(ormExecutor.GetType(), ormExecutor, nameof(ORMExecutor.AlterTableIfChanged), new Type[] { currentSchema.Value }, new Type[] { }, new object[] { });
                }
            }

            // drop schemas
            foreach (var existingSchema in existingSchemas)
            {
                // if exists in current context then continue next
                if (currentSchemas.TryGetValue(existingSchema.TableName, out Type type)) continue;

                ormExecutor.DropTable(existingSchema.TableName);
            }
        }

        public void DatabaseInitialize<T>(T obj) where T : ORMDatabase
        {
            DatabaseInitialize<T>();
        }

        private void CreateDatabaseIfNotExists()
        {
            try
            {
                var connSB = new SqlConnectionStringBuilder(_connectionString);
                var dbName = connSB.InitialCatalog;
                connSB.InitialCatalog = string.Empty;

                using (SqlConnection conn = new SqlConnection(connSB.ConnectionString))
                {
                    conn.Open();
                    var query = $"SELECT NAME FROM SYS.DATABASES WHERE NAME = '{dbName}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var res = cmd.ExecuteScalar();

                        // database is not exists, so create database
                        if (res == null)
                        {
                            var createDB = $"CREATE DATABASE {dbName}";
                            cmd.CommandText = createDB;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("SQL database couldn't be opened or created", ex);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
