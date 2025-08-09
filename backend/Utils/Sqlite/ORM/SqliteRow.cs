using System.Reflection;

namespace Utils.Sqlite.ORM
{
    public abstract class SqliteRow
    {
    }

    public enum SqliteColumnType
    {
        Integer,
        Text,
        Real,
        DateTime
    }

    public enum SqliteNull
    {
        Null,
        NotNull
    }

    public enum SqliteUniqueKey
    {
        None,
        UniqueKey
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteColumn : Attribute
    {
        public SqliteColumnType Type;
        public SqliteNull NullConstraint;
        public SqliteUniqueKey UniqueKey;

        public SqliteColumn(
            SqliteColumnType type, SqliteNull nullConstraint = SqliteNull.Null, SqliteUniqueKey uniqueKey = SqliteUniqueKey.None
        )
        {
            Type = type;
            NullConstraint = nullConstraint;
            UniqueKey = uniqueKey;
        }
    }

    public class SqliteColumnProp
    {
        public SqliteColumn Attribute;
        private PropertyInfo Property;

        public SqliteColumnProp(SqliteColumn attribute, PropertyInfo property)
        {
            Attribute = attribute;
            Property = property;
        }

        public string GetColName()
        {
            return Property.Name;
        }

        public object GetSqlValue(SqliteRow row)
        {
            var val = Property.GetValue(row);
            if (val is null) throw new Exception("unable to extract property value for: " + Property.Name);
            return val;
        }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class SqliteTable : Attribute
    {
        public string TableName;

        public SqliteTable(string tableName)
        {
            TableName = tableName;
        }
    }
}
