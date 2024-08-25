using Ardalis.Specification;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Specification;

public static class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> WithoutDerivedTypes<T>(
        this ISpecificationBuilder<T> specificationBuilder)
    {
        specificationBuilder.Specification.Items[QueryableExtensions.WithoutDerivedTypesTag<T>()] = true;
        return specificationBuilder;
    }
}
