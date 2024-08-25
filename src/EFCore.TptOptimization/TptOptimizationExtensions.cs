using System.Globalization;
using JustCSharp.EntityFrameworkCore.TptOptimization.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JustCSharp.EntityFrameworkCore.TptOptimization;

public static class TptOptimizationExtensions
{
    public static DbContextOptionsBuilder UseTptOptimization(
        this DbContextOptionsBuilder optionsBuilder,
        CultureInfo? culture = null)
    {
        var extension = (optionsBuilder.Options.FindExtension<TptOptimizationExtension>()
                         ?? new TptOptimizationExtension());

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    public static DbContextOptionsBuilder<TContext> UseTptOptimization<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder , CultureInfo? culture = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseTptOptimization((DbContextOptionsBuilder)optionsBuilder, culture);
}
