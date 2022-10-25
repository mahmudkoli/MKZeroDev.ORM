using System.Data;
using System.Text;
using System.Text.Json;

namespace MKZeroDev.ORM
{
    internal class QueryGenerator<T> where T : class
    {
        private readonly string _tableName = default!;

        public string TableName { get { return _tableName; } }

        public QueryGenerator()
        {
            _tableName = ObjectExtensions.GetDbTableName<T>();
        }

        #region Generate Database Object
        public ORMInformation GenerateTableSchema()
        {
            var tableName = _tableName;
            var primaryKeys = ObjectExtensions.GetDbPrimaryKeys<T>();
            var columns = new List<ORMInformationColumn>();

            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this table, maybe added in another table with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);
                columns.Add(new ORMInformationColumn() { ColumnName = dbColDef.ColumnName, ColumnType = dbColDef.ColumnType, IsNullable = dbColDef.IsNullable });
            }

            var res = new ORMInformation(tableName, JsonSerializer.Serialize(columns), string.Join(',', primaryKeys));

            return res;
        }

        public IList<ORMInformationColumn> GenerateColumnsSchema(string columnsJson)
        {
            return JsonSerializer.Deserialize<IList<ORMInformationColumn>>(columnsJson).ToList();
        }

        public string GenerateCreateTableCommand()
        {
            StringBuilder querySB = new StringBuilder();
            querySB.AppendLine($"CREATE TABLE {_tableName} (");

            var type = typeof(T);
            var properties = type.GetProperties();
            var primaryKeys = ObjectExtensions.GetDbPrimaryKeys<T>();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this table, maybe added in another table with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);
                var colDef = $"\t{dbColDef.ColumnName} {dbColDef.ColumnType} {(dbColDef.IsNullable ? "NULL" : "NOT NULL")},";

                if (index == properties.Length - 1 && !primaryKeys.Any()) colDef = colDef.TrimEnd(',');
                querySB.AppendLine(colDef);
            }

            // primary keys added if exists
            if (primaryKeys.Any())
                querySB.AppendLine($"\tPRIMARY KEY ({string.Join(',', primaryKeys)})");

            querySB.AppendLine(")");

            return querySB.ToString();
        }

        public string GenerateAlterTableCommand(
                IList<ORMInformationColumn> addColumns,
                IList<ORMInformationColumn> dropColumns,
                IList<ORMInformationColumn> modifyColumns
            )
        {
            StringBuilder querySB = new StringBuilder();

            // add columns
            if (addColumns.Any())
            {
                querySB.AppendLine($"ALTER TABLE {_tableName}");
                querySB.AppendLine($"\tADD");

                foreach (var (column, index) in addColumns.Select((column, index) => (column, index)))
                {
                    var colDef = $"\t\t{column.ColumnName} {column.ColumnType} {(column.IsNullable ? "NULL" : "NOT NULL")},";

                    if (index == addColumns.Count() - 1)
                    {
                        colDef = colDef.TrimEnd(',');
                        colDef += ";";
                    }
                    querySB.AppendLine(colDef);
                }
            }
            querySB.AppendLine();

            // drop columns
            if (dropColumns.Any())
            {
                querySB.AppendLine($"ALTER TABLE {_tableName}");
                querySB.AppendLine($"\tDROP COLUMN");

                foreach (var (column, index) in dropColumns.Select((column, index) => (column, index)))
                {
                    var colDef = $"\t\t{column.ColumnName},";

                    if (index == dropColumns.Count() - 1)
                    {
                        colDef = colDef.TrimEnd(',');
                        colDef += ";";
                    }
                    querySB.AppendLine(colDef);
                }
            }
            querySB.AppendLine();

            // modify columns
            if (modifyColumns.Any())
            {
                querySB.AppendLine($"ALTER TABLE {_tableName}");
                querySB.AppendLine($"\tALTER COLUMN");

                foreach (var (column, index) in modifyColumns.Select((column, index) => (column, index)))
                {
                    var colDef = $"\t\t{column.ColumnName} {column.ColumnType} {(column.IsNullable ? "NULL" : "NOT NULL")},";

                    if (index == modifyColumns.Count() - 1)
                    {
                        colDef = colDef.TrimEnd(',');
                        colDef += ";";
                    }
                    querySB.AppendLine(colDef);
                }
            }
            querySB.AppendLine();

            return querySB.ToString();
        }

        public string GenerateDropTableCommand()
        {
            StringBuilder querySB = new StringBuilder();
            querySB.AppendLine($"DROP TABLE {_tableName}");
            return querySB.ToString();
        }

        public string GenerateDropTableCommand(string tableName)
        {
            StringBuilder querySB = new StringBuilder();
            querySB.AppendLine($"DROP TABLE {tableName}");
            return querySB.ToString();
        }
        #endregion

        #region Generate CRUD
        public string GenerateSelectAllQuery()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var columnNames = new List<string>();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);

                columnNames.Add($"{dbColDef.ColumnName}");
            }

            return $"SELECT {string.Join(", ", columnNames)} FROM {_tableName}";
        }

        public string GenerateSelectFirstOrDefaultQuery()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var columnNames = new List<string>();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);

                columnNames.Add($"{dbColDef.ColumnName}");
            }

            return $"SELECT TOP 1 {string.Join(", ", columnNames)} FROM {_tableName}";
        }

        public (string Query, Dictionary<string, object?> ParamsValues) GenerateInsertQuery(T obj)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var columnNames = new List<string>();
            var columnParamsValues = new Dictionary<string, object?>();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);
                var columnValue = property.GetValue(obj);

                columnNames.Add(dbColDef.ColumnName);
                columnParamsValues.Add($"@pi__{index}", columnValue);
            }

            var query = $"INSERT INTO {_tableName} ({string.Join(", ", columnNames)}) Values ({string.Join(", ", columnParamsValues.Keys.ToList())})";

            return (query, columnParamsValues);
        }

        public (string Query, Dictionary<string, object?> ParamsValues) GenerateUpdateQuery(T obj)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var updateColumnParams = new Dictionary<string, string>();
            var columnParamsValues = new Dictionary<string, object?>();

            foreach (var (property, index) in properties.Select((property, index) => (property, index)))
            {
                if (ObjectExtensions.IsGenericOrClassRef(property)) continue; // not added in this query, maybe added in another query with relation
                var dbColDef = ObjectExtensions.GetDbColumnDef(property);
                var columnValue = property.GetValue(obj);

                updateColumnParams.Add(dbColDef.ColumnName, $"@pu__{index}");
                columnParamsValues.Add($"@pu__{index}", columnValue);
            }

            var primaryKeysValues = ObjectExtensions.GetDbPrimaryKeysValues<T>(obj);
            var whereColumnParams = new Dictionary<string, string>();
            foreach (var (primaryKeyValue, index) in primaryKeysValues.Select((primaryKeyValue, index) => (primaryKeyValue, index)))
            {
                whereColumnParams.Add(primaryKeyValue.Key, $"@pw__{index}");
                columnParamsValues.Add($"@pw__{index}", primaryKeyValue.Value);
            }

            var query = $"UPDATE {_tableName} SET {string.Join(", ", updateColumnParams.Select(kv => $"{kv.Key} = {kv.Value}"))} WHERE {string.Join(" and ", whereColumnParams.Select(kv => $"{kv.Key} = {kv.Value}"))}";

            return (query, columnParamsValues);
        }

        public (string Query, Dictionary<string, object?> ParamsValues) GenerateDeleteQuery(T obj)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var columnParamsValues = new Dictionary<string, object?>();

            var primaryKeysValues = ObjectExtensions.GetDbPrimaryKeysValues<T>(obj);
            var whereColumnParams = new Dictionary<string, string>();
            foreach (var (primaryKeyValue, index) in primaryKeysValues.Select((primaryKeyValue, index) => (primaryKeyValue, index)))
            {
                whereColumnParams.Add(primaryKeyValue.Key, $"@pw__{index}");
                columnParamsValues.Add($"@pw__{index}", primaryKeyValue.Value);
            }

            var query = $"DELETE FROM {_tableName} WHERE {string.Join(" and ", whereColumnParams.Select(kv => $"{kv.Key} = {kv.Value}"))}";

            return (query, columnParamsValues);
        }
        #endregion
    }
}
