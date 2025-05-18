using Microsoft.EntityFrameworkCore;

namespace MusicGQL.Db;

public static class DbContextExtensions
{
    public static void AttachKnownEntities(this DbContext context, object entity)
    {
        var visited = new HashSet<object>();

        void Visit(object obj)
        {
            if (obj == null || visited.Contains(obj))
                return;

            var type = obj.GetType();

            if (IsSimpleType(type))
                return;

            visited.Add(obj);

            var entityType = context.Model.FindEntityType(type);
            if (entityType != null)
            {
                var key = entityType.FindPrimaryKey();
                if (key != null)
                {
                    var keyValues = key
                        .Properties.Select(p => p.PropertyInfo?.GetValue(obj))
                        .ToArray();

                    if (keyValues.All(v => v != null))
                    {
                        // Check if already being tracked
                        var tracked = context
                            .ChangeTracker.Entries()
                            .FirstOrDefault(e =>
                                e.Entity.GetType() == type
                                && key.Properties.All(p =>
                                    Equals(
                                        p.PropertyInfo?.GetValue(e.Entity),
                                        p.PropertyInfo?.GetValue(obj)
                                    )
                                )
                            );

                        if (tracked != null)
                        {
                            return; // already tracked, skip
                        }

                        // Check if already exists in DB
                        var dbEntity = context.Find(type, keyValues);
                        if (dbEntity != null)
                        {
                            context.Entry(obj).State = EntityState.Detached;
                            context.Attach(dbEntity);
                            return;
                        }
                    }
                }
            }

            foreach (var prop in type.GetProperties())
            {
                if (
                    typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType)
                    && prop.PropertyType != typeof(string)
                )
                {
                    if (prop.GetValue(obj) is IEnumerable<object> collection)
                    {
                        foreach (var item in collection)
                        {
                            Visit(item);
                        }
                    }
                }
                else if (context.Model.FindEntityType(prop.PropertyType) != null)
                {
                    Visit(prop.GetValue(obj));
                }
            }
        }

        Visit(entity);
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid)
            || type == typeof(TimeSpan);
    }
}
