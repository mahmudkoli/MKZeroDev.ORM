namespace MKZeroDev.ORM
{
    internal class ORMExecutor
    {
        private readonly SQLExecutor _sqlExecutor;

        public ORMExecutor(string connectionString)
        {
            _sqlExecutor = new SQLExecutor(connectionString);
        }

        #region Schema
        public IList<ORMInformation> GetExistingDBSchemas()
        {
            var schemaGenerator = new QueryGenerator<ORMInformation>();
            var schemaQuery = schemaGenerator.GenerateSelectAllQuery();
            var res = _sqlExecutor.GetAll<ORMInformation>(schemaQuery);
            return res;
        }

        public int CreateTable<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            if (_sqlExecutor.IsExistsTable(generator.TableName)) throw new Exception($"Already exists table {generator.TableName}");
            var command = generator.GenerateCreateTableCommand();
            var res = _sqlExecutor.ExecuteCommand(command);

            if (typeof(T) != typeof(ORMInformation))
                InsertORMInformation<T>();

            return res;
        }
        
        public int CreateTable<T>(T obj) where T : class
        {
            return CreateTable<T>();
        }

        public int CreateTableIfNotExists<T>() where T : class
        {
            var generator = new QueryGenerator<T>();

            if (!_sqlExecutor.IsExistsTable(generator.TableName))
            {
                var command = generator.GenerateCreateTableCommand();
                var res = _sqlExecutor.ExecuteCommand(command);

                if (typeof(T) != typeof(ORMInformation))
                    InsertORMInformation<T>();

                return res;
            }

            return 0;
        }
        
        public int CreateTableIfNotExists<T>(T obj) where T : class
        {
            return CreateTableIfNotExists<T>(obj);
        }

        public int AlterTableIfChanged<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            if (!_sqlExecutor.IsExistsTable(generator.TableName)) throw new Exception($"Table {generator.TableName} not exists");

            // existing schemas from database
            var schemaGenerator = new QueryGenerator<ORMInformation>();
            var schemaQuery = schemaGenerator.GenerateSelectAllQuery();
            var existingSchemas = _sqlExecutor.GetAll<ORMInformation>(schemaQuery);

            // current schema
            var currentSchema = generator.GenerateTableSchema();
            var currentColumns = generator.GenerateColumnsSchema(currentSchema.ColumnsJson);
            var currentPrimaryKeys = currentSchema.PrimaryKeys.Split(',').ToList();

            // existing schema
            var existingSchema = existingSchemas.FirstOrDefault(x => x.TableName == generator.TableName);
            var existingColumns = generator.GenerateColumnsSchema(existingSchema.ColumnsJson);
            var existingPrimaryKeys = currentSchema.PrimaryKeys.Split(',').ToList();

            var addColumns = currentColumns.Where(cc => !existingColumns.Any(ec => ec.ColumnName == cc.ColumnName)).ToList();
            var dropColumns = existingColumns.Where(cc => !currentColumns.Any(ec => ec.ColumnName == cc.ColumnName)).ToList();
            var modifyColumns = currentColumns.Where(cc => existingColumns.Any(ec => ec.ColumnName == cc.ColumnName && (ec.ColumnType != cc.ColumnType || ec.IsNullable != cc.IsNullable))).ToList();

            if (addColumns.Any() || dropColumns.Any() || modifyColumns.Any())
            {
                var command = generator.GenerateAlterTableCommand(addColumns, dropColumns, modifyColumns);
                var res = _sqlExecutor.ExecuteCommand(command);
                UpdateORMInformation<T>();
                return res;
            }

            return 0;
        }

        public int AlterTableIfChanged<T>(T obj) where T : class
        {
            return AlterTableIfChanged<T>();
        }

        public int DropTable<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            if (!_sqlExecutor.IsExistsTable(generator.TableName)) throw new Exception($"Table {generator.TableName} not exists");
            var command = generator.GenerateDropTableCommand();
            var res = _sqlExecutor.ExecuteCommand(command);
            DeleteORMInformation<T>();
            return res;
        }

        public int DropTable<T>(T obj) where T : class
        {
            return DropTable<T>();
        }

        public int DropTable(string tableName)
        {
            var generator = new QueryGenerator<ORMQGTest>();
            if (!_sqlExecutor.IsExistsTable(tableName)) throw new Exception($"Table {tableName} not exists");
            var command = generator.GenerateDropTableCommand(tableName);
            var res = _sqlExecutor.ExecuteCommand(command);
            DeleteORMInformation(tableName);
            return res;
        }
        #endregion

        #region Data
        public int Insert<T>(T item) where T : class
        {
            var generator = new QueryGenerator<T>();
            var query = generator.GenerateInsertQuery(item);
            var res = _sqlExecutor.ExecuteCommand(query.Query, query.ParamsValues);
            return res;
        }

        public int Update<T>(T item) where T : class
        {
            var generator = new QueryGenerator<T>();
            var query = generator.GenerateUpdateQuery(item);
            var res = _sqlExecutor.ExecuteCommand(query.Query, query.ParamsValues);
            return res;
        }

        public int Delete<T>(T item) where T : class
        {
            var generator = new QueryGenerator<T>();
            var query = generator.GenerateDeleteQuery(item);
            var res = _sqlExecutor.ExecuteCommand(query.Query, query.ParamsValues);
            return res;
        }

        public IList<T> SelectAll<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            var command = generator.GenerateSelectAllQuery();
            var res = _sqlExecutor.GetAll<T>(command);
            return res;
        }

        public T? SelectFirstOrDefault<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            var command = generator.GenerateSelectFirstOrDefaultQuery();
            var res = _sqlExecutor.GetFirstOrDefault<T>(command);
            return res;
        }
        #endregion

        #region ORM Log
        private void InsertORMInformation<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            var schema = generator.GenerateTableSchema();
            var item = new ORMInformation(schema.TableName, schema.ColumnsJson, schema.PrimaryKeys);

            var res = Insert<ORMInformation>(item);
        }

        private void UpdateORMInformation<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            var schema = generator.GenerateTableSchema();
            var item = new ORMInformation(schema.TableName, schema.ColumnsJson, schema.PrimaryKeys);

            var res = Update<ORMInformation>(item);
        }

        private void DeleteORMInformation<T>() where T : class
        {
            var generator = new QueryGenerator<T>();
            var schema = generator.GenerateTableSchema();
            var item = new ORMInformation(schema.TableName, schema.ColumnsJson, schema.PrimaryKeys);

            var res = Delete<ORMInformation>(item);
        }

        private void DeleteORMInformation(string tableName)
        {
            var item = new ORMInformation(tableName, string.Empty, string.Empty);
            var res = Delete<ORMInformation>(item);
        }
        #endregion
    }
}
