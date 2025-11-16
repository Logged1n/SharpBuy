namespace Application.IntegrationTests;

/// <summary>
/// Base class for application integration tests with database fixture
/// </summary>
[Collection(nameof(DatabaseCollection))]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected ApplicationDbContext DbContext = null!;

    protected BaseIntegrationTest(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
        DbContext = Fixture.CreateDbContext();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
    }
}

/// <summary>
/// Collection definition for sharing database fixture across test classes
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
