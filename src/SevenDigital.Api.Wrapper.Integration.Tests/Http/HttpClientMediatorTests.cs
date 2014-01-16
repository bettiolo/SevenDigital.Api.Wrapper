using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Integration.Tests.Http
{
	[TestFixture]
	public class HttpClientMediatorTests
	{
		private const string ApiUrl = "http://api.7digital.com/1.2";
		private string consumerKey;

		[SetUp]
		public void Setup()
		{
			consumerKey = new AppSettingsCredentials().ConsumerKey;
		}

		[Test]
		public async void Can_resolve_uri()
		{
			string url = string.Format("{0}/status?oauth_consumer_key={1}", ApiUrl, consumerKey);
			var request = new GetRequest(url,  new Dictionary<string, string>());

			var response = await new HttpClientMediator().GetAsync(request);
			AssertResponse(response, HttpStatusCode.OK);
		}

		[Test]
		public async void Bad_url_should_return_not_found()
		{
			string url = string.Format("{0}/foo/bar/fish/1234?oauth_consumer_key={1}", ApiUrl, consumerKey);
			var request = new GetRequest(url, new Dictionary<string, string>());

			var response = await new HttpClientMediator().GetAsync(request);
			AssertResponse(response, HttpStatusCode.NotFound);
		}

		[Test]
		public async void No_key_should_return_unauthorized()
		{
			string url = string.Format("{0}/status", ApiUrl);
			var request = new GetRequest(url, new Dictionary<string, string>());

			var response = await new HttpClientMediator().GetAsync(request);
			AssertResponse(response, HttpStatusCode.Unauthorized);
		}

		[Test]
		[Ignore("There was a NullReferenceException that this test catches, however we don't enable this by default because:" +
		"1: It would slow down the build a lot." + 
		"2: It would depend on a hanging-web.app being set up for the test.")]
		public async void Can_cope_with_timeouts()
		{
			var apiUrl = "http://hanging-web-app.7digital.local";
			var request = new GetRequest(apiUrl, new Dictionary<string, string>());

			var response = await new HttpClientMediator().GetAsync(request);
			AssertResponse(response, HttpStatusCode.OK);
		}

		[Test]
		public async void bad_url_post__should_return_not_found()
		{
			string url = string.Format("{0}/foo/bar/fish/1234?oauth_consumer_key={1}", ApiUrl, consumerKey);
			var parameters = new Dictionary<string, string>
				{
					{"foo", "bar"}
				};

			var request = new PostRequest(url, new Dictionary<string, string>(), parameters.ToQueryString());

			var response = await new HttpClientMediator().PostAsync(request);
			AssertResponse(response, HttpStatusCode.NotFound);
		}

		private static void AssertResponse(Response response, HttpStatusCode expectedCode)
		{
			Assert.That(response, Is.Not.Null, "No response");
			Assert.That(response.StatusCode, Is.EqualTo(expectedCode), "Unexpected http status code");
			Assert.That(response.Headers.Count, Is.GreaterThan(0), "No headers found");
			Assert.That(response.Body, Is.Not.Empty, "No response body found");
		}
	}
}
