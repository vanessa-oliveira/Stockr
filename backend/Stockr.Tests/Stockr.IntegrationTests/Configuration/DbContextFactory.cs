using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockr.Infrastructure.Context;

namespace Stockr.IntegrationTests.Configuration;

public static class DbContextFactory
{
    public static DataContext CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(connection)
            .Options;

        var context = new DataContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}