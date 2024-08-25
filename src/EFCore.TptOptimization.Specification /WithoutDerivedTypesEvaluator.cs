using Ardalis.Specification;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Specification;

public class WithoutDerivedTypesEvaluator : IEvaluator
{
    public static WithoutDerivedTypesEvaluator Instance { get; } = new();

    private WithoutDerivedTypesEvaluator() { }

    public bool IsCriteriaEvaluator => true;

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
    {
        if (specification.Items.ContainsKey(QueryableExtensions.WithoutDerivedTypesTag<T>()))
        {
            return query.WithoutDerivedTypes();
        }

        return query;
    }
}
