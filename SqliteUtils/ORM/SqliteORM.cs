using Dapper;
using Microsoft.Data.Sqlite;
using SqliteUtils.Cache;
using System.Reflection;
using System.Text;

namespace SqliteUtils.ORM
{
    public class SqliteORM<T> : IDisposable where T : SqliteRow
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly SqliteConnection connection;
        private bool initCheck = false;
        private bool disposedValue;

        public SqliteORM(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir == null) throw new Exception("Unable to get directory from path: " + path);
            Directory.CreateDirectory(dir);
            connection = new SqliteConnection($"Data Source={path};");
        }

        /// <summary>
        /// Gets values matching the given filter clause.
        /// </summary>
        public IEnumerable<T> Get(string filter, object? args = null)
        {
            CheckInit();
            var tableName = GetTableName();
            var sql = new StringBuilder();
            sql.AppendLine($"select *");
            sql.AppendLine($"from {tableName}");
            sql.AppendLine($"where {filter}");
            var cmd = new CommandDefinition(sql.ToString(), args);
            return connection.Query<T>(cmd).ToArray();
        }

        /// <summary>
        /// Inserts values if given values have null primary key id or given primary keys don't exist in db.
        /// If primary key id exists, db is updated to match given value.
        /// </summary>
        public void Upsert(IEnumerable<T> values)
        {
            CheckInit();
            /* 
            https://www.sqlite.org/lang_UPSERT.html
            */
            var sql = new StringBuilder();
            var args = new Dictionary<string, object>();
            var tableName = GetTableName();
            var cols = GetSqliteColumns();

            sql.AppendLine($"insert into {tableName}({string.Join(", ", cols.Select(x => x.GetColName()))})");
            sql.AppendLine($"values");
            var valIdx = 0;
            foreach (var val in values)
            {
                var paramNames = new List<string>();
                foreach (var col in cols)
                {
                    var param = $"@{col.GetColName()}_{valIdx}";
                    args[param] = col.GetSqlValue(val);
                    paramNames.Add(param);
                }
                sql.AppendLine($"({string.Join(", ", paramNames)})");
                valIdx++;
            }
            var uniqueKeyCols = cols.Where(x => x.Attribute.UniqueKey == SqliteUniqueKey.UniqueKey).ToArray();
            if (uniqueKeyCols.Length > 0)
            {
                sql.AppendLine($"on conflict({string.Join(", ", uniqueKeyCols.Select(x => x.GetColName()))}) do update set");
            }
            var otherCols = cols.Where(x => x.Attribute.UniqueKey == SqliteUniqueKey.None).ToArray();
            foreach (var col in otherCols)
            {
                var colname = col.GetColName();
                sql.AppendLine($"{colname}=excluded.{colname}{(col == otherCols.Last() ? "" : ",")}");
            }

            var cmd = new CommandDefinition(sql.ToString(), new DynamicParameters(args), flags: CommandFlags.NoCache);
            connection.Execute(cmd);
        }

        /// <summary>
        /// Inserts values if given values have null primary key id or given primary keys don't exist in db.
        /// If primary key id exists, db is updated to match given value.
        /// </summary>
        public void Upsert(T value)
        {
            Upsert(new T[] { value });
        }

        /// <summary>
        /// Deletes values who's unique key ids match the given values.
        /// If values are null or don't exist, nothing occurs.
        /// </summary>
        public void Delete(IEnumerable<T> values)
        {
            CheckInit();
            throw new NotImplementedException("TODO WESD");
        }

        /// <summary>
        /// Deletes values matching the given filter.
        /// </summary>
        public void Delete(string filter)
        {
            CheckInit();
            throw new NotImplementedException("TODO WESD");
        }

        /// <summary>
        /// Runs first time initialization if not run already.
        /// </summary>
        protected virtual void CheckInit()
        {
            if (!initCheck)
            {
                var tableName = GetTableName();

                var sql = new StringBuilder();
                sql.AppendLine($"create table if not exists {tableName}(");
                // add id primary key
                sql.AppendLine($"{tableName}Id Integer Not Null Primary Key Autoincrement,");
                var cols = GetSqliteColumns();
                // add columns
                foreach (var col in cols)
                {
                    var attr = col.Attribute;
                    var sqliteType = attr.Type switch
                    {
                        SqliteColumnType.Integer => "Integer",
                        SqliteColumnType.Text => "Text",
                        SqliteColumnType.Real => "Real",
                        SqliteColumnType.DateTime => "DateTime",
                        _ => throw new Exception($"unhandled sqlite col type: {attr.Type}")
                    };
                    sql.Append($"{col.GetColName()} {sqliteType}");
                    if (attr.NullConstraint == SqliteNull.NotNull)
                    {
                        sql.Append($" NOT NULL");
                    }
                    sql.AppendLine($",");
                }
                // add constraints
                var uniqueKeyCols = cols.Where(x => x.Attribute.UniqueKey == SqliteUniqueKey.UniqueKey).ToArray();
                if (uniqueKeyCols.Length > 0)
                {
                    sql.AppendLine($"Unique({string.Join(", ", uniqueKeyCols.Select(x => x.GetColName()))})");
                }
                sql.AppendLine($")");
                // can keep caching as this should not change per application run.
                connection.Execute(sql.ToString());
            }
            initCheck = true;
        }

        /// <summary>
        /// Iterates over properties of the class marked with SqliteColumn metadata
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SqliteColumnProp> GetSqliteColumns()
        {
            Type type = typeof(T);

            //iterating through the method attribtues
            foreach (var property in type.GetProperties())
            {
                foreach (var attr in property.GetCustomAttributes(true).Where(x => x is SqliteColumn).Cast<SqliteColumn>())
                {
                    // TODO WESD implement some basic type checks to make sure property type and sqlite type match? or get rid of sqlite type and infer based on C# type?
                    yield return new SqliteColumnProp(attr, property);
                }
            }
        }

        /// <summary>
        /// Iterates over properties of the class marked with SqliteColumn metadata
        /// </summary>
        /// <returns></returns>
        private string GetTableName()
        {
            Type type = typeof(T);
            var tableAttr = type.GetCustomAttributes().Where(x => x is SqliteTable).Cast<SqliteTable>().FirstOrDefault();
            if (tableAttr == null) throw new Exception($"SqliteTable attribute missing from {type.Name}.");

            return tableAttr.TableName;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                connection?.Dispose();
                // set large fields to null
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SqliteORM()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
