using TheCell.Bibaboulder.Sharedtests;

namespace TheCell.Bibaboulder.Integrationtests;

[CollectionDefinition(nameof(CollectionForIntegrationTests))]
public class CollectionForIntegrationTests : ICollectionFixture<IntegrationTestFactory>
{
}
