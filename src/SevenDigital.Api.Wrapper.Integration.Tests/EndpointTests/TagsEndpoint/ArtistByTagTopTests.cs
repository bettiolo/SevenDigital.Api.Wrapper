using System.Linq;
using NUnit.Framework;
using SevenDigital.Api.Schema;
using SevenDigital.Api.Schema.Tags;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.TagsEndpoint
{
	[TestFixture]
	public class ArtistByTagTopTests
	{
		[Test]
		public async void Can_hit_endpoint()
		{

			ArtistByTagTop tags = await Api<ArtistByTagTop>.Create
				.WithParameter("tags", "rock,pop,2000s")
				.PleaseAsync();

			Assert.That(tags, Is.Not.Null);
			Assert.That(tags.TaggedArtists.Count, Is.GreaterThan(0));
			Assert.That(tags.Type, Is.EqualTo(ItemType.artist));
			Assert.That(tags.TaggedArtists.FirstOrDefault().Artist.Name, Is.Not.Empty);
		}

		[Test]
		public async void Can_hit_endpoint_with_paging()
		{

			ArtistByTagTop artistBrowse = await Api<ArtistByTagTop>.Create
				.WithParameter("tags", "rock,pop,2000s")
				.WithParameter("page", "2")
				.WithParameter("pageSize", "20")
				.PleaseAsync();

			Assert.That(artistBrowse, Is.Not.Null);
			Assert.That(artistBrowse.Page, Is.EqualTo(2));
			Assert.That(artistBrowse.PageSize, Is.EqualTo(20));
		}
	}
}