using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Internal;

internal static class TptOptimizationServiceCollectionExtensions
{
    internal const string DecoratedKey = "Decorated";
    
    internal static IServiceCollection AddEntityFrameworkTptOptimization(
        this IServiceCollection serviceCollection)
    {
        DecorateService<IQueryTranslationPreprocessorFactory, TptOptimizationQueryTranslationPreprocessorFactory>(serviceCollection);
        DecorateService<IShapedQueryCompilingExpressionVisitorFactory, TptOptimizationShapedQueryCompilingExpressionVisitorFactory>(serviceCollection);
        
        return serviceCollection;
    }
    
    private static void DecorateService<TService, TDecorator>(IServiceCollection serviceCollection)
        where TService : class
        where TDecorator : class, TService 
    {
        var originalServiceDescriptor = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(TService));
    
        if (originalServiceDescriptor is null || originalServiceDescriptor.ImplementationType is null) 
        {
            throw new Exception("You need to register DbProvider before using TptOptimization");
        }
    
        var decoratedServiceDescriptor = new ServiceDescriptor(originalServiceDescriptor.ServiceType, DecoratedKey,
            originalServiceDescriptor.ImplementationType, originalServiceDescriptor.Lifetime);
    
        serviceCollection.Remove(originalServiceDescriptor);
        serviceCollection.Add(decoratedServiceDescriptor);
        
        serviceCollection.TryAddScoped<TService, TDecorator>();
    }
}
