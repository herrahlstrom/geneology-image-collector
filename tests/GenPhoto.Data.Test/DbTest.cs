using FluentAssertions;

namespace GeneologyImageCollectorTest;

[TestClass]
public class DbTest
{
    private AppDbContext? db;

    [TestCleanup]
    public void TestCleanup()
    {
        db?.Dispose();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        var dbOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        dbOptionsBuilder.UseInMemoryDatabase("test-db");

        db = new AppDbContext(dbOptionsBuilder.Options);
        var created = db.Database.EnsureCreated();
        created.Should().BeTrue();
    }
}
