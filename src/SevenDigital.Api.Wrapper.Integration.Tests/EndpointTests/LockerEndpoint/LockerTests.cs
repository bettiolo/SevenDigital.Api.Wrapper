using System.Linq;
using NUnit.Framework;
using SevenDigital.Api.Schema.LockerEndpoint;

namespace SevenDigital.Api.Wrapper.Integration.Tests.EndpointTests.LockerEndpoint
{
	[TestFixture]
	public class LockerTests
	{
		private readonly string _token = TestDataFromEnvironmentOrAppSettings.AccessToken;
		private readonly string _tokenSecret = TestDataFromEnvironmentOrAppSettings.AccessTokenSecret;

		[SetUp]
		public void RunOnce()
		{
			if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_tokenSecret))
				Assert.Ignore("these tests need an access token and secret to run");
		}

		[Test]
		public async void Should_get_a_users_locker_with_correct_access_credentials()
		{
			var locker = await Api<Locker>.Create
				.ForUser(_token, _tokenSecret)
				.PleaseAsync();

			Assert.That(locker.Response.LockerReleases.Count, Is.GreaterThan(0));
		}

		[Test]
		public async void Should_get_specific_users_release()
		{
			var locker = await Api<Locker>.Create
				.ForReleaseId(2830094)
				.ForUser(_token, _tokenSecret)
				.PleaseAsync();

			Assert.That(locker.Response.LockerReleases.Count, Is.EqualTo(1));
		}

		[Test]
		public async void Should_get_specific_users_track()
		{
			var locker = await Api<Locker>.Create
				.ForReleaseId(2830094)
				.ForTrackId(30317180)
				.ForUser(_token, _tokenSecret)
				.PleaseAsync();

			Assert.That(locker.Response.LockerReleases.FirstOrDefault().LockerTracks.Count, Is.EqualTo(1));
		}
	}
}