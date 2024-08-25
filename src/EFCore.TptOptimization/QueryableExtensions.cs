using Microsoft.EntityFrameworkCore;

namespace JustCSharp.EntityFrameworkCore.TptOptimization;

public static class QueryableExtensions
{
    internal const string WithoutDerivedTypesTagPrefix = "TptOptimization.WithoutDerivedTypes";

    internal static string WithoutDerivedTypesTag(Type type) => $"{WithoutDerivedTypesTagPrefix}:{type.FullName}";
    internal static string WithoutDerivedTypesTag<T>() => WithoutDerivedTypesTag(typeof(T));
    
    public static IQueryable<T> WithoutDerivedTypes<T>(this IQueryable<T> queryable) where T : class
    {
        return queryable.TagWith(WithoutDerivedTypesTag<T>());
    }
}
