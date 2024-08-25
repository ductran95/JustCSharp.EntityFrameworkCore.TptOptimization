using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Sqlite.FunctionalTests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public virtual DbSet<Parent> Parents { get; set; }
    public virtual DbSet<Children> Childrens { get; set; }
    public virtual DbSet<GrandChildren> GrandChildrens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parent>().UseTptMappingStrategy();
    }
}

public class Parent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class Children : Parent
{
    public string ChildrenName { get; set; } = string.Empty;
}

public class GrandChildren : Children
{
    public string GrandChildrenName { get; set; } = string.Empty;
}
