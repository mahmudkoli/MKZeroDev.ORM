namespace MKZeroDev.ORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        private readonly string _tableName = default!;
        public string TableName { get { return _tableName; } }

        public TableNameAttribute(string tableName)
        {
            _tableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnDefAttribute : Attribute
    {
        private readonly string _columnName = default!;
        private readonly string _columnType = default!;
        public string ColumnName { get { return _columnName; } }
        public string ColumnType { get { return _columnType; } }

        public ColumnDefAttribute(string columnName = "", string columnType = "")
        {
            _columnName = columnName;
            _columnType = columnType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnRequiredAttribute : Attribute
    {
        public ColumnRequiredAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        private readonly string _propertyName = default!; // Navigation or Foreign Key
        public string PropertyName { get { return _propertyName; } } // Navigation or Foreign Key

        public ForeignKeyAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }
    }
}
