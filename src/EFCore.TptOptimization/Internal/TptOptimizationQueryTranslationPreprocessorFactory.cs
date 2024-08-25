using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Internal;

internal class TptOptimizationQueryTranslationPreprocessorFactory : IQueryTranslationPreprocessorFactory
{
    public TptOptimizationQueryTranslationPreprocessorFactory(QueryTranslationPreprocessorDependencies dependencies,
        [FromKeyedServices(TptOptimizationServiceCollectionExtensions.DecoratedKey)]
        IQueryTranslationPreprocessorFactory inner)
    {
        Dependencies = dependencies;
        Inner = inner;
    }

    protected virtual QueryTranslationPreprocessorDependencies Dependencies { get; }
    protected virtual IQueryTranslationPreprocessorFactory Inner { get; }

    public QueryTranslationPreprocessor Create(QueryCompilationContext queryCompilationContext)
    {
        var innerPreprocessor = Inner.Create(queryCompilationContext);
        return new TptOptimizationQueryTranslationPreprocessor(Dependencies, queryCompilationContext,
            innerPreprocessor);
    }
}

internal class TptOptimizationQueryTranslationPreprocessor : QueryTranslationPreprocessor
{
    public TptOptimizationQueryTranslationPreprocessor(QueryTranslationPreprocessorDependencies dependencies,
        QueryCompilationContext queryCompilationContext, QueryTranslationPreprocessor inner) : base(dependencies,
        queryCompilationContext)
    {
        Inner = inner;
    }

    protected virtual QueryTranslationPreprocessor Inner { get; }

    public override Expression Process(Expression query)
    {
        query = Inner.Process(query);
        query = new WithoutDerivedTypesTagVisitor(QueryCompilationContext).Visit(query);
        if (QueryCompilationContext.Tags.Any(x => x.StartsWith(QueryableExtensions.WithoutDerivedTypesTagPrefix)))
        {
            query = new WithoutDerivedTypesEntityVisitor(QueryCompilationContext).Visit(query);
        }

        return query;
    }
}

internal class WithoutDerivedTypesTagVisitor : ExpressionVisitor
{
    internal static readonly MethodInfo TagWithMethodInfo
        = typeof(EntityFrameworkQueryableExtensions).GetMethod(
            nameof(EntityFrameworkQueryableExtensions.TagWith),
            new[] { typeof(IQueryable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)), typeof(string) })!;

    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    public WithoutDerivedTypesTagVisitor(QueryCompilationContext queryCompilationContext)
    {
        QueryCompilationContext = queryCompilationContext;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var method = node.Method;

        if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
            && method.IsGenericMethod
            && ExtractQueryMetadata(node) is Expression expression)
        {
            return expression;
        }

        return base.VisitMethodCall(node);
    }

    private Expression? ExtractQueryMetadata(MethodCallExpression methodCallExpression)
    {
        // We visit innerQueryable first so that we can get information in the same order operators are applied.
        var genericMethodDefinition = methodCallExpression.Method.GetGenericMethodDefinition();

        if (genericMethodDefinition == TagWithMethodInfo)
        {
            var visitedExpression = Visit(methodCallExpression.Arguments[0]);
            var tag = GetConstantValue<string>(methodCallExpression.Arguments[1]);

            if (tag.StartsWith(QueryableExtensions.WithoutDerivedTypesTagPrefix))
            {
                QueryCompilationContext.AddTag(tag);
            }

            return visitedExpression;
        }

        return null;
    }

    public static T GetConstantValue<T>(Expression expression)
        => expression is ConstantExpression constantExpression
            ? (T)constantExpression.Value!
            : throw new InvalidOperationException();
}

internal class WithoutDerivedTypesEntityVisitor : ExpressionVisitor
{
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    public WithoutDerivedTypesEntityVisitor(QueryCompilationContext queryCompilationContext)
    {
        QueryCompilationContext = queryCompilationContext;
    }
    
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Arguments[0] is EntityQueryRootExpression entityQueryRootExpression 
            && entityQueryRootExpression.EntityType is RuntimeEntityType runtimeEntityType
            && QueryCompilationContext.Tags.Contains(QueryableExtensions.WithoutDerivedTypesTag(runtimeEntityType.ClrType)))
        {
            if (runtimeEntityType.DirectlyDerivedTypes.Any())
            {
                var directlyDerivedTypes = new SortedSet<RuntimeTypeBase>();
                foreach (var derivedType in runtimeEntityType.DirectlyDerivedTypes)
                {
                    directlyDerivedTypes.Add(derivedType);
                }
                runtimeEntityType.SetRuntimeAnnotation(QueryableExtensions.WithoutDerivedTypesTagPrefix, directlyDerivedTypes);
                runtimeEntityType.DirectlyDerivedTypes.Clear();
            }
            
        }
    
        return base.VisitMethodCall(node);
    }
}
