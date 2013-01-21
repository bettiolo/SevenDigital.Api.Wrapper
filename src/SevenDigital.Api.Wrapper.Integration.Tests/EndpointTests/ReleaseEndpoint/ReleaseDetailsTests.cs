using NUnit.Framework;
using SevenDigital.Api.Schema.ReleaseEndpoint;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.ReleaseEndpoint
{
	[TestFixture]
	[Category("Integration")]
	public class ReleaseDetailsTests
	{
		[Test]
		public async void Can_hit_endpoint()
		{
			Release release = await Api<Release>.Create
				.WithParameter("releaseId", "155408")
				.WithParameter("country", "GB")
				.PleaseAsync();

			Assert.That(release, Is.Not.Null);
			Assert.That(release.Title, Is.EqualTo("Dreams"));
			Assert.That(release.Artist.Name, Is.EqualTo("The Whitest Boy Alive"));
		}

	}
}