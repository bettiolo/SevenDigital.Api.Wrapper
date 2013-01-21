using System.Linq;
using NUnit.Framework;
using SevenDigital.Api.Schema.Merchandising;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.MerchandisingEndpoint
{
	[TestFixture]
	public class MerchandisingListEndpointTests
	{
		[Test, Ignore("In beta testing")]
		public async void Can_hit_fluent_endpoint_for_merchandising()
		{
			var merchList = await Api<MerchandisingList>
				.Create
				.WithKey("tabAlbums")
				.WithParameter("shopId", "34")
				.PleaseAsync();

			Assert.That(merchList, Is.Not.Null);
			Assert.That(merchList.Key, Is.EqualTo("tabAlbums"));
			Assert.That(merchList.Items.Count, Is.GreaterThan(1));

			var merchandisingListItem = merchList.Items.First();
			Assert.That(merchandisingListItem.Release.Title, Is.Not.Null);
		}
	}
}
