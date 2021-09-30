using RSSMS.DataService.Attributes;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace RSSMS.DataService.Utilities
{
    public static class LinQUtils
    {
        public static IQueryable<TEntity> DynamicFilter<TEntity>(this IQueryable<TEntity> source, TEntity entity)
        {
            var properties = entity.GetType().GetProperties();
            foreach (var item in properties)
            {
                if (entity.GetType().GetProperty(item.Name) == null) continue;
                var propertyVal = entity.GetType().GetProperty(item.Name).GetValue(entity, null);
                if (propertyVal == null) continue;
                if (item.CustomAttributes.Any(a => a.AttributeType == typeof(SkipAttribute))) continue;
                bool isDateTime = item.PropertyType == typeof(DateTime);
                if (isDateTime)
                {
                    DateTime dt = (DateTime)propertyVal;
                    source = source.Where($"{item.Name} >= @0 && { item.Name} < @1", dt.Date, dt.Date.AddDays(1));
                }
                else if (item.CustomAttributes.Any(a => a.AttributeType == typeof(ContainAttribute)))
                {
                    var array = (IList)propertyVal;
                    source = source.Where($"{item.Name}.Any(a=> @0.Contains(a))", array);
                }
                else if (item.CustomAttributes.Any(a => a.AttributeType == typeof(StringAttribute)))
                {
                    source = source.Where($"{item.Name}.ToLower().Contains(@0)", propertyVal.ToString().ToLower());
                }
                else if (item.PropertyType == typeof(string))
                {
                    source = source.Where($"{item.Name} = \"{((string)propertyVal).Trim()}\"");
                }
                else
                {
                    source = source.Where($"{item.Name} = {propertyVal}");
                }
            }
            return source;
        }
        public static (int, IQueryable<TResult>) PagingIQueryable<TResult>(this IQueryable<TResult> source, int page, int size, int limitPaging, int defaultPaging)
        {
            if (size > limitPaging) size = limitPaging;
            if (size < 1) size = defaultPaging;
            if (page < 1) page = 1;
            int total = source.Count();
            IQueryable<TResult> results = source.Skip((page - 1) * size)
                                            .Take(size);
            return (total, results);
        }
        public static string ToDynamicSelector<TEntity>(this string[] selectorArray)
        {
            var selectors = selectorArray.Where(a => !string.IsNullOrEmpty(a)).Select(s => s.SnakeCaseToLower()).ToList();
            var entityProperties = typeof(TEntity).GetProperties().Select(s => s.Name.SnakeCaseToLower()).ToArray();
            entityProperties = entityProperties.Where(w => selectors.Contains(w)).ToArray();
            return @"new {" + string.Join(',', entityProperties) + "}";
        }
        public static string SnakeCaseToLower(this string o)
        {
            return o.Contains("-") ? string.Join(string.Empty, o.Split("-")).Trim().ToLower() : string.Join(string.Empty, o.Split("-")).Trim().ToLower();
        }
    }
}
