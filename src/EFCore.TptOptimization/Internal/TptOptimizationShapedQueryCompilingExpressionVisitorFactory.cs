using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Internal;

internal class
    TptOptimizationShapedQueryCompilingExpressionVisitorFactory : IShapedQueryCompilingExpressionVisitorFactory
{
    public TptOptimizationShapedQueryCompilingExpressionVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        [FromKeyedServices(TptOptimizationServiceCollectionExtensions.DecoratedKey)]
        IShapedQueryCompilingExpressionVisitorFactory inner)
    {
        Dependencies = dependencies;
        Inner = inner;
    }

    protected virtual ShapedQueryCompilingExpressionVisitorDependencies Dependencies { get; }
    protected virtual IShapedQueryCompilingExpressionVisitorFactory Inner { get; }

    public ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
    {
        if (queryCompilationContext.Tags.Any(x => x.StartsWith(QueryableExtensions.WithoutDerivedTypesTagPrefix)))
        {
            var innerPreprocessor = Inner.Create(queryCompilationContext);
            return new TptOptimizationShapedQueryCompilingExpressionVisitor(Dependencies, queryCompilationContext,
                innerPreprocessor);
        }
        
        return Inner.Create(queryCompilationContext);
    }
}

internal class TptOptimizationShapedQueryCompilingExpressionVisitor : ShapedQueryCompilingExpressionVisitor
{
    private int _callStackDepth;
    private Expression? _originalExpression;
    private readonly ReturnDerivedTypesEntityVisitor _returnDerivedTypesEntityVisitor;
    
    public TptOptimizationShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        ShapedQueryCompilingExpressionVisitor inner) : base(dependencies, queryCompilationContext)
    {
        Inner = inner;
        _returnDerivedTypesEntityVisitor = new ReturnDerivedTypesEntityVisitor(queryCompilationContext);
    }

    protected virtual ShapedQueryCompilingExpressionVisitor Inner { get; }

    public override Expression? Visit(Expression? node)
    {
        if (_callStackDepth == 0)
        {
            _originalExpression = node;
        }
        
        _callStackDepth++;
        var result = Inner.Visit(node);
        _callStackDepth--;

        if (_callStackDepth == 0)
        {
            _returnDerivedTypesEntityVisitor.Visit(_originalExpression);
        }

        return result;
    }

    protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
    {
        throw new NotImplementedException();
    }
}

internal class ReturnDerivedTypesEntityVisitor : ExpressionVisitor
{
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    public ReturnDerivedTypesEntityVisitor(QueryCompilationContext queryCompilationContext)
    {
        QueryCompilationContext = queryCompilationContext;
    }
    
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
        {
            Visit(shapedQueryExpression.QueryExpression);
            Visit(shapedQueryExpression.ShaperExpression);
    
            return extensionExpression;
        }

        if (extensionExpression is TableExpression tableExpression)
        {
            foreach (var entityTypeMapping in tableExpression.Table.EntityTypeMappings)
            {
                if (entityTypeMapping.TypeBase is RuntimeEntityType runtimeEntityType &&
                    QueryCompilationContext.Tags.Contains(
                        QueryableExtensions.WithoutDerivedTypesTag(runtimeEntityType.ClrType)))
                {
                    var annotationValue = runtimeEntityType.FindRuntimeAnnotation(QueryableExtensions.WithoutDerivedTypesTagPrefix);
            
                    if (annotationValue != null && annotationValue.Value is SortedSet<RuntimeEntityType> directlyDerivedTypes)
                    {
                        foreach (var derivedType in directlyDerivedTypes)
                        {
                            runtimeEntityType.DirectlyDerivedTypes.Add(derivedType);
                        }

                        runtimeEntityType.RemoveRuntimeAnnotation(QueryableExtensions.WithoutDerivedTypesTagPrefix);
                    }
                }
            }
        }
    
        return base.VisitExtension(extensionExpression);
    }
}
