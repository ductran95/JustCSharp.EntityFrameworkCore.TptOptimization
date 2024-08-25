using Microsoft.EntityFrameworkCore;

namespace JustCSharp.EntityFrameworkCore.TptOptimization.Sqlite.FunctionalTests;

public class QueryTests : IClassFixture<SqliteDatabaseFixture>
{
    private readonly SqliteDatabaseFixture _fixture;

    public QueryTests(SqliteDatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task QueryNormalSet()
    {
        using var dbContext = _fixture.CreateDbContext();
        
        var data = await dbContext.Parents.AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.NotNull(data);
        Assert.NotEmpty(data);
        Assert.Equal(3, data.Count);
        Assert.IsType<Parent>(data[0]);
        Assert.IsType<Children>(data[1]);
        Assert.IsType<GrandChildren>(data[2]);
    }
    
    [Fact]
    public async Task QueryOptimizedSet()
    {
        using var dbContext = _fixture.CreateDbContext();
        
        var data = await dbContext.Parents.AsNoTracking().WithoutDerivedTypes()
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        Assert.NotNull(data);
        Assert.NotEmpty(data);
        Assert.Equal(3, data.Count);
        Assert.IsType<Parent>(data[0]);
        Assert.IsNotType<Children>(data[1]);
        Assert.IsNotType<GrandChildren>(data[2]);
    }
    
    [Fact]
    public async Task Combine()
    {
        using var dbContext = _fixture.CreateDbContext();
        
        var simpleData = await dbContext.Parents.AsNoTracking().WithoutDerivedTypes()
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        Assert.NotNull(simpleData);
        Assert.NotEmpty(simpleData);
        Assert.Equal(3, simpleData.Count);
        Assert.IsType<Parent>(simpleData[0]);
        Assert.IsNotType<Children>(simpleData[1]);
        Assert.IsNotType<GrandChildren>(simpleData[2]);
        
        var fullData = await dbContext.Parents.AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        Assert.NotNull(fullData);
        Assert.NotEmpty(fullData);
        Assert.Equal(3, fullData.Count);
        Assert.IsType<Parent>(fullData[0]);
        Assert.IsType<Children>(fullData[1]);
        Assert.IsType<GrandChildren>(fullData[2]);
    }
}
