using SqliteUtils.ORM;

namespace SqliteUtils.Cache
{
    [SqliteTable("Cache")]
    public class CacheRow : SqliteRow
    {
        [SqliteColumn(SqliteColumnType.Text, SqliteNull.NotNull, SqliteUniqueKey.UniqueKey)]
        public string? CacheKey { get; set; }
        [SqliteColumn(SqliteColumnType.Text, SqliteNull.NotNull)]
        public string? CacheVal { get; set; }
        [SqliteColumn(SqliteColumnType.DateTime)]
        public DateTime? DateSet { get; set; }
    }
}
