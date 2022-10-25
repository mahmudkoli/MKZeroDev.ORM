namespace MKZeroDev.ORM
{
    [TableName("ORMInformations")]
    public class ORMInformation
    {
        [PrimaryKey]
        [ColumnDef(columnType: "nvarchar(50)")]
        public string TableName { get; set; } = default!;
        public string ColumnsJson { get; set; } = default!;
        public string PrimaryKeys { get; set; } = default!;

        public ORMInformation()
        {

        }

        public ORMInformation(string tableName, string columnsJson, string primaryKeys)
        {
            TableName = tableName;
            ColumnsJson = columnsJson;
            PrimaryKeys = primaryKeys;
        }
    }

    public class ORMInformationColumn
    {
        public string ColumnName { get; set; } = default!;
        public string ColumnType { get; set; } = default!;
        public bool IsNullable { get; set; } = default!;
    }

    // just use for create a query generator instance incase of don't knowing class
    public class ORMQGTest
    {

    }
}
