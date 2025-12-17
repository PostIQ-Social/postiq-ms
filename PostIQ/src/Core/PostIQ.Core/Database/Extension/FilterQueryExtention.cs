using PostIQ.Core.Database.Entities;
using PostIQ.Core.Database.Helpers;
using LinqKit;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using static PostIQ.Core.Database.Entities.FilterUtility;

namespace PostIQ.Core.Database.Extension
{
    public static class FilterQueryExtention
    {
        #region Filter
        public static Expression<Func<T, bool>> Filter<T>(this IQueryable<T> query, List<FilterModel> filters)
        {
            var predicate = PredicateBuilder.New<T>();
            if (filters.Any())
            {
                foreach (var rows in filters)
                {
                    if (rows.Filters.Any())
                    {
                        foreach (var filter in rows.Filters)
                        {
                            //parent logic applied
                            predicate = rows.Logic.ToLower() == "or" ? predicate.Or(FilterExp<T>(filter.Filters, filter.Logic))
                                : rows.Logic.ToLower() == "and" ? predicate.And(FilterExp<T>(filter.Filters, filter.Logic))
                                : predicate.And(FilterExp<T>(filter.Filters, filter.Logic));
                        }
                    }
                }
            }
            return predicate;
        }

        private static Expression<Func<T, bool>> FilterExp<T>(List<FilterParams> filterParams, string logic)
        {
            //child logic applied - can be single or multiple
            var predicate = PredicateBuilder.New<T>();
            Expression<Func<T, bool>> expression = null;
            foreach (var parm in filterParams)
            {
                var filterColumn = typeof(T).GetProperty(parm.Field, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                if (filterColumn != null)
                {
                    expression = FilterData<T>(filterColumn, parm.Value, parm.Operator);
                    if (expression != null)
                    {
                        if (string.IsNullOrEmpty(logic))
                        {
                            predicate = predicate.And(expression);
                        }
                        else if (logic.ToLower() == "and")
                        {
                            predicate = predicate.And(expression);
                        }
                        else if (logic.ToLower() == "or")
                        {
                            predicate = predicate.Or(expression);
                        }
                    }
                }
            }
            return predicate;
        }
        private static Expression<Func<T, bool>> FilterData<T>(PropertyInfo prop, string value, string filterOptions)
        {
            try
            {
                Expression expMethod = null;
                string[] numOperation = { "equals", "equal", "notequals", "notequal", "greater", "greaterequal", "less", "lessequal",
                    "eq", "neq", "lt", "lte", "gt", "gte", "isnull", "isnotnull", "isempty", "isnotempty" };
                string[] commOperation = { "isnull", "isnotnull", "isempty", "isnotempty" };
                string[] strOperation = { "startwith", "endwith", "contains", "notcontains", "doesnotcontains", "endsWith", "startswith", "endswith" };
                string[] inOperation = { "in" };

                var parameter = Expression.Parameter(typeof(T));
                Expression memberAccess = Expression.Property(parameter, prop);
                int outIntValue = 0;
                decimal outDesValue = 0;
                DateTime outDateValue = new DateTime();

                bool propertyIsNullableValueType = memberAccess.Type.IsGenericType && memberAccess.Type.GetGenericTypeDefinition() == typeof(Nullable<>);
                Type propertyBasicType = propertyIsNullableValueType ? memberAccess.Type.GetGenericArguments().Single() : memberAccess.Type;

                string Operation = filterOptions.ToString();
                ConstantExpression constant;
                if (numOperation.Contains(Operation, StringComparer.OrdinalIgnoreCase))
                {
                    object convertedValue;
                    if (commOperation.Contains(Operation, StringComparer.OrdinalIgnoreCase))
                    {
                        value = null;
                    }
                    if (value == null || propertyBasicType.IsInstanceOfType(value))
                    {
                        convertedValue = value;
                    }
                    else
                    {
                        convertedValue = ConverterHelper.ConvertStringToType(value, propertyBasicType);
                    }
                    if (convertedValue == null && memberAccess.Type.IsValueType && !propertyIsNullableValueType)
                    {
                        Type nullableMemberType = typeof(Nullable<>).MakeGenericType(memberAccess.Type);
                        memberAccess = Expression.Convert(memberAccess, nullableMemberType);
                    }
                    constant = Expression.Constant(convertedValue, memberAccess.Type);
                }
                else if (strOperation.Contains(Operation, StringComparer.OrdinalIgnoreCase))
                {
                    constant = Expression.Constant(value.ToString(), typeof(string));
                }
                else if (inOperation.Contains(Operation, StringComparer.OrdinalIgnoreCase))
                {
                    string[] values = value.Split(',').ToArray();
                    var objValues = ConverterHelper.ConvertStringArrayToTypeArray(values, propertyBasicType);
                    return InExpression<T>(prop, objValues);
                }
                else
                {
                    throw new Exception($"unsupported generic filter option '{Operation}' on a property");
                }
               
                switch (filterOptions.ToLower())
                {
                    //case FilterOptions.startwith:
                    case "startwith":
                    case "startswith":
                        expMethod = Expression.Call(memberAccess
                                                , typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })
                                                , constant);
                        break;
                    //case FilterOptions.endwith:
                    case "endwith":
                    case "endsWith":
                        expMethod = Expression.Call(memberAccess
                                                , typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })
                                                , constant);
                        break;
                    //case FilterOptions.contains:
                    //case FilterOptions.doesnotcontain:
                    case "contains":
                    case "doesnotcontain":
                    case "notcontains":
                        var cconMethod = Expression.Call(memberAccess
                                                , typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })
                                                , constant);
                        if (Operation.Equals(FilterOptions.contains.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            expMethod = cconMethod;
                        }
                        if (Operation.Equals(FilterOptions.doesnotcontain.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            expMethod = Expression.Not(cconMethod);
                        }
                        break;

                    //case FilterOptions.isnotnull:
                    //case FilterOptions.isnotempty:
                    //case FilterOptions.isempty:
                    //case FilterOptions.isnull:
                    case "isnotnull":
                    case "isnotempty":
                    case "isempty":
                    case "isnull":
                        Expression empMethod = null;
                        if (propertyBasicType == typeof(string))
                        {
                            empMethod = Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, memberAccess);
                        }
                        else
                        {
                            empMethod = Expression.Equal(memberAccess, constant);
                        }
                        if (Operation.Equals(FilterOptions.isempty.ToString(), StringComparison.OrdinalIgnoreCase)
                            || Operation.Equals(FilterOptions.isnull.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            expMethod = empMethod;
                        }
                        if (Operation.Equals(FilterOptions.isnotempty.ToString(), StringComparison.OrdinalIgnoreCase)
                            || Operation.Equals(FilterOptions.isnotnull.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            expMethod = Expression.Not(empMethod);
                        }
                        break;

                    //case FilterOptions.gt:
                    case "gt":
                        expMethod = Expression.GreaterThan(memberAccess, constant);
                        break;
                    //case FilterOptions.gte:
                    case "gte":
                        expMethod = Expression.GreaterThanOrEqual(memberAccess, constant);
                        break;
                    //case FilterOptions.lt:
                    case "lt":
                        expMethod = Expression.LessThan(memberAccess, constant);
                        break;
                    //case FilterOptions.lte:
                    case "lte":
                        expMethod = Expression.LessThanOrEqual(memberAccess, constant);
                        break;
                    //case FilterOptions.eq:
                    case "eq":
                    case "equal":
                    case "equals":
                        if (value == string.Empty && propertyBasicType == typeof(string))
                        {
                            expMethod = Expression.Call(memberAccess
                                               , typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) })
                                               , constant);
                        }
                        else
                        {
                            expMethod = Expression.Equal(memberAccess, constant);
                        }
                        break;
                    //case FilterOptions.neq:
                    case "neq":
                    case "notequals":
                        expMethod = Expression.NotEqual(memberAccess, constant);
                        break;

                }
                return Expression.Lambda<Func<T, bool>>(expMethod, parameter);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static DateTime ParseJsonDateTime(string text, string infoPropertyName, Type infoPropertyType)
        {
            string dateString = "\"" + text.Replace("/", "\\/") + "\"";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dateString)))
            {
                var serializer = new DataContractJsonSerializer(typeof(DateTime));
                try
                {
                    return (DateTime)serializer.ReadObject(stream);
                }
                catch (Exception ex)
                {

                    throw new Exception($"Invalid json format of {infoPropertyType.Name} propert '{infoPropertyName}'", ex);
                }
            }
        }

        #endregion

        #region Sort

        public static IQueryable<T> Sort<T>(this IQueryable<T> query, List<SortParam> sorts)
        {
            foreach (var sort in sorts)
            {
                query = AddSorting(query, sort.Dir, sort.Field);
            }
            return query;
        }
        private static IQueryable<TEntity> AddSorting<TEntity>(IQueryable<TEntity> query, string sortDirection, string propertyName)
        {
            var param = Expression.Parameter(typeof(TEntity));
            var prop = Expression.PropertyOrField(param, propertyName);
            var sortLambda = Expression.Lambda(prop, param);

            Expression<Func<IOrderedQueryable<TEntity>>> sortMethod = null;

            switch (sortDirection)
            {
                case "asc" when query.Expression.Type == typeof(IOrderedQueryable<TEntity>):
                    sortMethod = () => ((IOrderedQueryable<TEntity>)query).ThenBy<TEntity, object>(k => null);
                    break;
                case "asc":
                    sortMethod = () => query.OrderBy<TEntity, object>(k => null);
                    break;
                case "desc" when query.Expression.Type == typeof(IOrderedQueryable<TEntity>):
                    sortMethod = () => ((IOrderedQueryable<TEntity>)query).ThenByDescending<TEntity, object>(k => null);
                    break;
                case "desc":
                    sortMethod = () => query.OrderByDescending<TEntity, object>(k => null);
                    break;
            }

            var methodCallExpression = (sortMethod.Body as MethodCallExpression);
            if (methodCallExpression == null)
                throw new Exception("MethodCallExpression null");

            var method = methodCallExpression.Method.GetGenericMethodDefinition();
            var genericSortMethod = method.MakeGenericMethod(typeof(TEntity), prop.Type);
            return (IOrderedQueryable<TEntity>)genericSortMethod.Invoke(query, new object[] { query, sortLambda });
        }


        public static IQueryable<T> SortQuery<T>(this IQueryable<T> query, List<SortParam> sorts)
        {
            if (sorts == null)
                return query;
            var applicableSorts = sorts.Where(s => s != null);
            if (!applicableSorts.Any())
                return query;
            applicableSorts.Select((item, index) => new { Index = index, item.Field, item.Dir }).ToList().ForEach(sort =>
            {
                ParameterExpression parameterExpression = Expression.Parameter(query.ElementType, "entity");
                var propertyExpression = Expression.Property(parameterExpression, sort.Field);
                var sortPredicate = Expression.Lambda(propertyExpression, parameterExpression);
                string methodName = (sort.Index == 0 ? "Order" : "Then") + (sort.Dir == "asc" ? "By" : "ByDesecending");
                MethodCallExpression orderBy = Expression.Call(typeof(Queryable),
                                                                methodName,
                                                                new Type[] { query.ElementType, propertyExpression.Type },
                                                                query.Expression,
                                                                Expression.Quote(sortPredicate));
                query = query.Provider.CreateQuery<T>(orderBy);
            });
            return query;
        }
        #endregion




        //public static Expression<Func<T, bool>> In<T>(string propertyName, IEnumerable<object> values)
        public static Expression<Func<T, bool>> InExpression<T>(PropertyInfo propertyInfo, Array values)
        {
            // Get the type of the entity
            var entityType = typeof(T);

            // Get the property information based on the property name
            //var propertyInfo = entityType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"'{propertyInfo.Name}' is not a valid property of type '{entityType.Name}'");
            }

            // Define the parameter for the lambda expression
            var parameter = Expression.Parameter(entityType, "x");

            // Define the property expression (e.g., x.PropertyName)
            var property = Expression.Property(parameter, propertyInfo);

            // Convert the values to the correct type
            //var typedValues = values.Select(value => Convert.ChangeType(value, propertyInfo.PropertyType)).ToArray();

            // Define the constant expression for the values (e.g., { value1, value2, ... })
            //var constant = Expression.Constant(typedValues);
            var constant = Expression.Constant(values);

            // Create the contains method call expression (e.g., new [] { value1, value2 }.Contains(x.PropertyName))
            var containsMethod = typeof(Enumerable).GetMethods().First(method =>
                    method.Name == "Contains" &&
                    method.GetParameters().Length == 2)
                .MakeGenericMethod(propertyInfo.PropertyType);

            var containsExpression = Expression.Call(containsMethod, constant, property);

            // Define the lambda expression (e.g., x => new [] { value1, value2 }.Contains(x.PropertyName))
            var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

            return lambda;
        }
    


    public static IQueryable<T> WhereIn<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> propertyExpression,
        IEnumerable<TKey> values)
        {
            if (values == null || !values.Any())
                return query; // Return the original query if the values are empty or null

            var constant = Expression.Constant(values.ToList());
            var body = Expression.Call(
                typeof(Enumerable),
                "Contains",
                new Type[] { typeof(TKey) },
                constant,
                propertyExpression
            );

            var predicate = Expression.Lambda<Func<T, bool>>(body, propertyExpression.Parameters);
            return query.Where(predicate);
        }
    }
}
