using Stockr.Infrastructure.Context;

namespace Stockr.IntegrationTests.Configuration;

public class DatabaseFixture : IDisposable
{
    public DataContext Context { get; private set; }

    public DatabaseFixture()
    {
        Context = DbContextFactory.CreateInMemoryDatabase();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }

    public void ResetDatabase()
    {
        Context.Dispose();
        Context = DbContextFactory.CreateInMemoryDatabase();
    }
}