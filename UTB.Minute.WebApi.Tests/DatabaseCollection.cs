using Xunit;

namespace UTB.Minute.WebApi.Tests;

[CollectionDefinition("Database collection", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<TestFixture>
{
}