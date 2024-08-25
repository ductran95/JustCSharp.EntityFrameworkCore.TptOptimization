using Microsoft.EntityFrameworkCore;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Sqlite.FunctionalTests;

public class SqliteDatabaseFixture : IDisposable
{
    private const string DbFile = "test.db";
    private const string ConnectionString = $"Data Source={DbFile}";

    private readonly TestDbContext _dbContext;
    
    public SqliteDatabaseFixture()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        dbContextOptionsBuilder.UseSqlite(ConnectionString);
        dbContextOptionsBuilder.UseTptOptimization();
        _dbContext = new TestDbContext(dbContextOptionsBuilder.Options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        var parent = new Parent()
        {
            Id = 1,
            Name = "Parent",
        };
        _dbContext.Add(parent);
        
        var children = new Children()
        {
            Id = 2,
            Name = "Children",
            ChildrenName = "Children own name"
        };
        _dbContext.Add(children);
        
        var grandChildren = new GrandChildren()
        {
            Id = 3,
            Name = "GrandChildren",
            ChildrenName = "Children name",
            GrandChildrenName = "GrandChildren own name"
        };
        _dbContext.Add(grandChildren);
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    public TestDbContext CreateDbContext()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        dbContextOptionsBuilder.UseSqlite(ConnectionString);
        dbContextOptionsBuilder.UseTptOptimization();
        return new TestDbContext(dbContextOptionsBuilder.Options);
    }
}