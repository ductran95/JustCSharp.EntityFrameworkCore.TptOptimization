using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Internal;

internal class TptOptimizationExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;
    
    public virtual DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);
    
    public void ApplyServices(IServiceCollection services) => services.AddEntityFrameworkTptOptimization();

    public void Validate(IDbContextOptions options)
    {
    }
    
    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) {}

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "using TPT optimization";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["UseTptOptimization"] = "1";
    }
}
