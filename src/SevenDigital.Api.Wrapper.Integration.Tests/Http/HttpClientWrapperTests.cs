using System.Linq;
using System.Collections.Generic;
using System.Net;

using NUnit.Framework;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Integration.Tests.Http
{
	[TestFixture]
	public class HttpClientWrapperTests
	{
		private const string ApiUrl = "http://api.7digital.com/1.2";
		private string _consumerKey;

		[SetUp]
		public void Setup()
		{
			_consumerKey = new AppSettingsCredentials().ConsumerKey;
		}

		[Test]
		public async void Can_resolve_uri()
		{
			string url = string.Format("{0}/status?oauth_consumer_key={1}", ApiUrl, _consumerKey);
			var headers = new Dictionary<string, string>();

			var httpClient = new HttpClientWrapper();
			var response = await httpClient.GetAsync(headers, url);

			AssertResponse(response, HttpStatusCode.OK);
		}

		[Test]
		public async void Bad_url_should_return_not_found()
		{
			string url = string.Format("{0}/foo/bar/fish/1234?oauth_consumer_key={1}", ApiUrl, _consumerKey);
			var headers = new Dictionary<string, string>();

			var httpClient = new HttpClientWrapper();
			var response = await httpClient.GetAsync(headers, url);

			AssertResponse(response, HttpStatusCode.NotFound);
		}

		[Test]
		public async void No_key_should_return_unauthorized()
		{
			string url = string.Format("{0}/status", ApiUrl);
			var headers = new Dictionary<string, string>();

			var httpClient = new HttpClientWrapper();
			var response = await httpClient.GetAsync(headers, url);

			AssertResponse(response, HttpStatusCode.Unauthorized);
		}

		[Test]
		public async void Bad_url_post_should_return_not_found()
		{
			string url = string.Format("{0}/foo/bar/fish/1234?oauth_consumer_key={1}", ApiUrl, _consumerKey);
			var headers = new Dictionary<string, string>();
			var parameters = new Dictionary<string, string>
				{
					{"foo", "bar"}
				};

			var httpClient = new HttpClientWrapper();
			var response = await httpClient.PostAsync(headers, parameters, url);

			AssertResponse(response, HttpStatusCode.NotFound);
		}

		private static void AssertResponse(Response response, HttpStatusCode expectedCode)
		{
			Assert.That(response, Is.Not.Null, "No response");
			Assert.That(response.StatusCode, Is.EqualTo(expectedCode), "Unexpected http status code");

			var headerNames = response.Headers.Keys.ToList();
			Assert.That(headerNames.Count, Is.GreaterThan(0), "No headers found");

			Assert.That(string.IsNullOrWhiteSpace(response.Body), Is.False, "No response body found");
		}
	}
}