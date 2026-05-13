using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Common;

[CollectionDefinition(CollectionName)]
public class IdentityTestCollection : ICollectionFixture<IdentityTestWebAppFactory>
{
    public const string CollectionName = "IdentityTestCollection";
}