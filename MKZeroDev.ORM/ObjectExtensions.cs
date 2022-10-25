using System.Reflection;

namespace MKZeroDev.ORM
{
    public static class ObjectExtensions
    {
        public static string GetDbTableName<T>()
        {
            var type = typeof(T);
            var tableName = type.Name;

            object[] attrs = type.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is TableNameAttribute tableNameAttr)
                {
                    tableName = tableNameAttr.TableName;
                }
            }

            return tableName;
        }

        public static string GetDbTableName<T>(T obj)
        {
            return GetDbTableName<T>();
        }
        
        public static (string ColumnName, string ColumnType, bool IsNullable) GetDbColumnDef(PropertyInfo property)
        {
            var columnName = property.Name;
            var columnType = GetDbType(property);

            object[] attrs = property.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is ColumnDefAttribute columnNameAttr)
                {
                    if (!string.IsNullOrEmpty(columnNameAttr.ColumnName))
                        columnName = columnNameAttr.ColumnName;

                    if (!string.IsNullOrEmpty(columnNameAttr.ColumnType))
                        columnType = columnNameAttr.ColumnType;
                }
            }

            return (columnName, columnType, IsNullable(property));
        }

        // call it if you don't pass value by using sql param
        public static object GetDbColumnValue<T>(T obj, PropertyInfo property)
        {
            var propValue = property.GetValue(obj);
            var propType = property.PropertyType;
            var typeCode = Type.GetTypeCode(propType);
            var columnValue = string.Empty;

            if (propValue != null)
            {
                switch (typeCode)
                {
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        columnValue = $"{propValue}";
                        break;
                    case TypeCode.String:
                        columnValue = $"N'{propValue}'";
                        break;
                    case TypeCode.Boolean:
                        columnValue = $"{(bool.Parse(propValue?.ToString() ?? string.Empty) ? 1 : 0)}";
                        break;
                    case TypeCode.Object:
                        if (propType == typeof(Guid) || propType == typeof(Guid?))
                        {
                            columnValue = $"'{propValue}'";
                        }
                        break;
                    default:
                        columnValue = $"{propValue}";
                        break;
                }
            }
            else
            {
                columnValue = "NULL";
            }

            return columnValue;
        }

        public static string GetDbType(PropertyInfo property, string? length = null)
        {
            var type = property.PropertyType;

            if (length == string.Empty) throw new ArgumentNullException("length should not be empty");

            if (type == typeof(string) || type == typeof(char[])) return $"nvarchar({(length ?? "max")})";
            else if (type == typeof(bool) || type == typeof(bool?)) return "bit";
            else if (type == typeof(byte) || type == typeof(byte?)) return "tinyint";
            else if (type == typeof(short) || type == typeof(short?)) return "smallint";
            else if (type == typeof(int) || type == typeof(int?)) return "int";
            else if (type == typeof(long) || type == typeof(long?)) return "bigint";
            else if (type == typeof(decimal) || type == typeof(decimal?)) return $"decimal({(length ?? "18,2")})";
            else if (type == typeof(float) || type == typeof(float?)) return "real";
            else if (type == typeof(double) || type == typeof(double?)) return "float";
            else if (type == typeof(DateTime) || type == typeof(DateTime?)) return "datetime2";
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) return "datetimeoffset";
            else if (type == typeof(Guid) || type == typeof(Guid?)) return "uniqueidentifier";
            else if (type == typeof(byte[])) return "varbinary";
            else if (type == typeof(char) || type == typeof(char?)) return "nvarchar(1)";
            else throw new ArgumentOutOfRangeException("Type");
        }

        public static bool IsNullable(PropertyInfo property)
        {
            var type = property.PropertyType;
            var isNullable = Nullable.GetUnderlyingType(type) != null;

            object[] attrs = property.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is ColumnRequiredAttribute columnRequiredAttr)
                {
                    isNullable = false;
                }
            }

            return isNullable;
        }

        public static bool IsGenericOrClassRef(PropertyInfo property)
        {
            var type = property.PropertyType;

            if (Nullable.GetUnderlyingType(type) != null)
                return false;
            else
                return type.IsGenericType || type.GetConstructor(new Type[0]) != null;
        }

        public static bool IsPrimaryKey(PropertyInfo property)
        {
            var isPrimaryKey = false;

            object[] attrs = property.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is PrimaryKeyAttribute primaryKeyAttr)
                {
                    isPrimaryKey = true;
                }
            }

            return isPrimaryKey;
        }

        public static List<string> GetDbPrimaryKeys<T>()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var primaryKeys = new List<string>();

            foreach (var property in properties)
            {
                object[] attrs = property.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (attr is PrimaryKeyAttribute primaryKeyAttr)
                    {
                        primaryKeys.Add(GetDbColumnDef(property).ColumnName);
                    }
                }
            }

            return primaryKeys;
        }

        public static Dictionary<string, object?> GetDbPrimaryKeysValues<T>(T obj)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var primaryKeysValues = new Dictionary<string, object?>();

            foreach (var property in properties)
            {
                object[] attrs = property.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (attr is PrimaryKeyAttribute primaryKeyAttr)
                    {
                        primaryKeysValues.Add(GetDbColumnDef(property).ColumnName, property.GetValue(obj));
                    }
                }
            }

            return primaryKeysValues;
        }

        public static bool HasDefaultConstructor(Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}


#region comment

//private static Type GetClrType(SqlDbType sqlType, bool isNullable)
//{
//    switch (sqlType)
//    {
//        case SqlDbType.BigInt:
//            return isNullable ? typeof(long?) : typeof(long);

//        case SqlDbType.Binary:
//        case SqlDbType.Image:
//        case SqlDbType.Timestamp:
//        case SqlDbType.VarBinary:
//            return typeof(byte[]);

//        case SqlDbType.Bit:
//            return isNullable ? typeof(bool?) : typeof(bool);

//        case SqlDbType.Char:
//        case SqlDbType.NChar:
//        case SqlDbType.NText:
//        case SqlDbType.NVarChar:
//        case SqlDbType.Text:
//        case SqlDbType.VarChar:
//        case SqlDbType.Xml:
//            return typeof(string);

//        case SqlDbType.DateTime:
//        case SqlDbType.SmallDateTime:
//        case SqlDbType.Date:
//        case SqlDbType.Time:
//        case SqlDbType.DateTime2:
//            return isNullable ? typeof(DateTime?) : typeof(DateTime);

//        case SqlDbType.Decimal:
//        case SqlDbType.Money:
//        case SqlDbType.SmallMoney:
//            return isNullable ? typeof(decimal?) : typeof(decimal);

//        case SqlDbType.Float:
//            return isNullable ? typeof(double?) : typeof(double);

//        case SqlDbType.Int:
//            return isNullable ? typeof(int?) : typeof(int);

//        case SqlDbType.Real:
//            return isNullable ? typeof(float?) : typeof(float);

//        case SqlDbType.UniqueIdentifier:
//            return isNullable ? typeof(Guid?) : typeof(Guid);

//        case SqlDbType.SmallInt:
//            return isNullable ? typeof(short?) : typeof(short);

//        case SqlDbType.TinyInt:
//            return isNullable ? typeof(byte?) : typeof(byte);

//        case SqlDbType.Variant:
//        case SqlDbType.Udt:
//            return typeof(object);

//        case SqlDbType.Structured:
//            return typeof(DataTable);

//        case SqlDbType.DateTimeOffset:
//            return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);

//        default:
//            throw new ArgumentOutOfRangeException("SqlDbType");
//    }
//}

//private static SqlDbType GetDbType(Type type, bool isNullable)
//{
//    switch (Type.GetTypeCode(type))
//    {
//        case TypeCode.Byte:
//            return SqlDbType.TinyInt;

//        case TypeCode.Int16:
//            return SqlDbType.SmallInt;

//        case TypeCode.Int32:
//            return SqlDbType.Int;

//        case TypeCode.Int64:
//            return SqlDbType.BigInt;

//        case TypeCode.Single:
//            return SqlDbType.Real;

//        case TypeCode.Double:
//            return SqlDbType.Float;

//        case TypeCode.Decimal:
//            return SqlDbType.Decimal;

//        case TypeCode.Boolean:
//            return SqlDbType.Bit;

//        case TypeCode.String:
//            return SqlDbType.NVarChar;

//        case TypeCode.Char:
//            return SqlDbType.Char;

//        case TypeCode.DateTime:
//            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
//                return SqlDbType.DateTimeOffset;
//            else return SqlDbType.DateTime;

//        case TypeCode.Object:
//            if (type == typeof(Guid) || type == typeof(Guid?))
//                return SqlDbType.UniqueIdentifier;
//            else if (type == typeof(byte[]))
//                return SqlDbType.VarBinary;
//            else throw new ArgumentOutOfRangeException("Type");

//        default:
//            throw new ArgumentOutOfRangeException("Type");
//    }

//}

//public static SqlDbType GetDBType(Type theType)
//{
//    SqlParameter sp = new SqlParameter();
//    TypeConverter tc = TypeDescriptor.GetConverter(sp.DbType);

//    if (tc.CanConvertFrom(theType))
//    {
//        sp.DbType = (DbType)tc.ConvertFrom(theType.Name);
//    }
//    else
//    {
//        //Try brute force
//        try
//        {
//            sp.DbType = (DbType)tc.ConvertFrom(theType.Name);
//        }
//        catch (Exception)
//        {
//            //Do Nothing; will return NVarChar as default
//        }
//    }
//    return sp.SqlDbType;
//}

//public static SqlDbType GetDBType1(Type theType)
//{
//    var typeMap = new Dictionary<Type, DbType>();
//    typeMap[typeof(byte)] = DbType.Byte;
//    typeMap[typeof(sbyte)] = DbType.SByte;
//    typeMap[typeof(short)] = DbType.Int16;
//    typeMap[typeof(ushort)] = DbType.UInt16;
//    typeMap[typeof(int)] = DbType.Int32;
//    typeMap[typeof(uint)] = DbType.UInt32;
//    typeMap[typeof(long)] = DbType.Int64;
//    typeMap[typeof(ulong)] = DbType.UInt64;
//    typeMap[typeof(float)] = DbType.Single;
//    typeMap[typeof(double)] = DbType.Double;
//    typeMap[typeof(decimal)] = DbType.Decimal;
//    typeMap[typeof(bool)] = DbType.Boolean;
//    typeMap[typeof(string)] = DbType.String;
//    typeMap[typeof(char)] = DbType.StringFixedLength;
//    typeMap[typeof(Guid)] = DbType.Guid;
//    typeMap[typeof(DateTime)] = DbType.DateTime;
//    typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
//    typeMap[typeof(byte[])] = DbType.Binary;
//    typeMap[typeof(byte?)] = DbType.Byte;
//    typeMap[typeof(sbyte?)] = DbType.SByte;
//    typeMap[typeof(short?)] = DbType.Int16;
//    typeMap[typeof(ushort?)] = DbType.UInt16;
//    typeMap[typeof(int?)] = DbType.Int32;
//    typeMap[typeof(uint?)] = DbType.UInt32;
//    typeMap[typeof(long?)] = DbType.Int64;
//    typeMap[typeof(ulong?)] = DbType.UInt64;
//    typeMap[typeof(float?)] = DbType.Single;
//    typeMap[typeof(double?)] = DbType.Double;
//    typeMap[typeof(decimal?)] = DbType.Decimal;
//    typeMap[typeof(bool?)] = DbType.Boolean;
//    typeMap[typeof(char?)] = DbType.StringFixedLength;
//    typeMap[typeof(Guid?)] = DbType.Guid;
//    typeMap[typeof(DateTime?)] = DbType.DateTime;
//    typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
//    typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
//}

//public static Type SqlToType(this string pSqlType)
//{
//    switch (pSqlType)
//    {
//        case "bigint":
//        case "real":
//            return typeof(long);
//        case "numeric":
//            return typeof(decimal);
//        case "bit":
//            return typeof(bool);

//        case "smallint":
//            return typeof(short);

//        case "decimal":
//        case "smallmoney":
//        case "money":
//            return typeof(decimal);

//        case "int":
//            return typeof(int);

//        case "tinyint":
//            return typeof(byte);

//        case "float":
//            return typeof(float);

//        case "date":
//        case "datetime2":
//        case "smalldatetime":
//        case "datetime":
//        case "time":
//            return typeof(DateTime);

//        case "datetimeoffset":
//            return typeof(DateTimeOffset);

//        case "char":
//        case "varchar":
//        case "text":
//        case "nchar":
//        case "nvarchar":
//        case "ntext":
//            return typeof(string);


//        case "binary":
//        case "varbinary":
//        case "image":
//            return typeof(byte[]);

//        case "uniqueidentifier":
//            return typeof(Guid);

//        default:
//            return typeof(string);

//    }

//}

//public static DbType ToDbType(this Type pType)
//{
//    switch (pType.Name.ToLower())
//    {
//        case "byte":
//            return DbType.Byte;
//        case "sbyte":
//            return DbType.SByte;
//        case "short":
//        case "int16":
//            return DbType.Int16;
//        case "uint16":
//            return DbType.UInt16;
//        case "int32":
//            return DbType.Int32;
//        case "uint32":
//            return DbType.UInt32;
//        case "int64":
//            return DbType.Int64;
//        case "uint64":
//            return DbType.UInt64;
//        case "single":
//            return DbType.Single;
//        case "double":
//            return DbType.Double;
//        case "decimal":
//            return DbType.Decimal;
//        case "bool":
//        case "boolean":
//            return DbType.Boolean;
//        case "string":
//            return DbType.String;
//        case "char":
//            return DbType.StringFixedLength;
//        case "Guid":
//            return DbType.Guid;
//        case "DateTime":
//            return DbType.DateTime;
//        case "DateTimeOffset":
//            return DbType.DateTimeOffset;
//        case "byte[]":
//            return DbType.Binary;
//        case "byte?":
//            return DbType.Byte;
//        case "sbyte?":
//            return DbType.SByte;
//        case "short?":
//            return DbType.Int16;
//        case "ushort?":
//            return DbType.UInt16;
//        case "int?":
//            return DbType.Int32;
//        case "uint?":
//            return DbType.UInt32;
//        case "long?":
//            return DbType.Int64;
//        case "ulong?":
//            return DbType.UInt64;
//        case "float?":
//            return DbType.Single;
//        case "double?":
//            return DbType.Double;
//        case "decimal?":
//            return DbType.Decimal;
//        case "bool?":
//            return DbType.Boolean;
//        case "char?":
//            return DbType.StringFixedLength;
//        case "Guid?":
//            return DbType.Guid;
//        case "DateTime?":
//            return DbType.DateTime;
//        case "DateTimeOffset?":
//            return DbType.DateTimeOffset;
//        default:
//            return DbType.String;
//    }

//}

#endregion
