using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests;

/// <summary>
/// Helper class to create mock DbSets for testing
/// </summary>
public static class DbSetMock
{
    public static DbSet<T> Create<T>(List<T>? data = null) where T : class
    {
        data ??= [];

        var queryable = data.AsQueryable();
        var dbSet = Substitute.For<DbSet<T>, IQueryable<T>>();

        ((IQueryable<T>)dbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<T>)dbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<T>)dbSet).ElementType.Returns(queryable.ElementType);
        ((IQueryable<T>)dbSet).GetEnumerator().Returns(queryable.GetEnumerator());

        dbSet.AsQueryable().Returns(queryable);

        // Mock Add method
        dbSet.When(x => x.Add(Arg.Any<T>()))
            .Do(x => data.Add(x.Arg<T>()));

        // Mock AddRange method
        dbSet.When(x => x.AddRange(Arg.Any<IEnumerable<T>>()))
            .Do(x => data.AddRange(x.Arg<IEnumerable<T>>()));

        // Mock Remove method
        dbSet.When(x => x.Remove(Arg.Any<T>()))
            .Do(x => data.Remove(x.Arg<T>()));

        return dbSet;
    }
}
