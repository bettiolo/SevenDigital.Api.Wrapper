using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Xml;
using FakeItEasy;
using NUnit.Framework;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.EndpointResolution.OAuth;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.Http
{
	[TestFixture]
	public class RequestCoordinatorTests
	{
		private const string ApiUrl = "http://api.7digital.com/1.2";
		private const string ServiceStatus = "<response status=\"ok\" version=\"1.2\" ><serviceStatus><serverTime>2011-03-04T08:10:29Z</serverTime></serviceStatus></response>";

		private readonly string _consumerKey = new AppSettingsCredentials().ConsumerKey;
		private IHttpClient _httpClient;
		private IRequestCoordinator _requestCoordinator;
		private IUrlSigner _urlSigner;

		[SetUp]
		public void Setup()
		{
			_httpClient = A.Fake<IHttpClient>();
			_urlSigner = A.Fake<IUrlSigner>();
			_requestCoordinator = new RequestCoordinator(_httpClient, _urlSigner, EssentialDependencyCheck<IOAuthCredentials>.Instance, EssentialDependencyCheck<IApiUri>.Instance);
		}

		[Test]
		public async void Should_fire_resolve_with_correct_values()
		{
			_httpClient.MockGetAsync(new Response(HttpStatusCode.OK, ServiceStatus));

			HttpMethod expectedMethod = HttpMethod.Get;
			var expectedHeaders = new Dictionary<string, string>();
			var expected = string.Format("{0}/test?oauth_consumer_key={1}", ApiUrl, _consumerKey);

			var endPointState = new RequestData
			{
				UriPath = "test",
				HttpMethod = expectedMethod,
				Headers = expectedHeaders
			};

			await _requestCoordinator
				.GetDataAsync(endPointState);

			_httpClient.GetAsyncOnUrlMustHaveHappened(expected);
		}

		[Test]
		public void Should_fire_resolve_with_url_encoded_parameters()
		{
			_httpClient.MockGetAsync(new Response(HttpStatusCode.OK, ServiceStatus));
			const string UnEncodedParameterValue = "Alive & Amplified";

			const string ExpectedParameterValue = "Alive%20%26%20Amplified";
			var expectedHeaders = new Dictionary<string, string>();
			var testParameters = new Dictionary<string, string> { { "q", UnEncodedParameterValue } };
			var expected = string.Format("{0}/test?oauth_consumer_key={1}&q={2}", ApiUrl, _consumerKey, ExpectedParameterValue);

			var endPointState = new RequestData
			{
				UriPath = "test",
				HttpMethod = HttpMethod.Get,
				Headers = expectedHeaders,
				Parameters = testParameters
			};

			_requestCoordinator.GetDataAsync(endPointState);

			_httpClient.GetAsyncOnUrlMustHaveHappened(expected);
		}

		[Test]
		public void Should_not_care_how_many_times_you_create_an_endpoint()
		{
			var endPointState = new RequestData
			{
				UriPath = "{slug}",
				HttpMethod = HttpMethod.Get,
				Parameters = new Dictionary<string, string> { { "slug", "something" } }
			};
			var result = _requestCoordinator.EndpointUrl(endPointState);

			Assert.That(result, Is.EqualTo(_requestCoordinator.EndpointUrl(endPointState)));
		}

		[Test]
		public async void Should_return_xmlnode_if_valid_xml_received()
		{
			Given_a_urlresolver_that_returns_valid_xml();

			var response = await _requestCoordinator
				.GetDataAsync(new RequestData());

			var hitEndpoint = new XmlDocument();
			hitEndpoint.LoadXml(response.Body);
			Assert.That(hitEndpoint.HasChildNodes);
			Assert.That(hitEndpoint.SelectSingleNode("//serverTime"), Is.Not.Null);
		}

		[Test]
		public async void Should_return_xmlnode_if_valid_xml_received_using_async()
		{
			var fakeClient = new FakeHttpClientWrapper(new Response(HttpStatusCode.OK, ServiceStatus));

			var endpointResolver = new RequestCoordinator(fakeClient, _urlSigner,
				EssentialDependencyCheck<IOAuthCredentials>.Instance, EssentialDependencyCheck<IApiUri>.Instance);

			var responseData = await endpointResolver
				.GetDataAsync(new RequestData());

			var response = responseData.Body;

			var payload = new XmlDocument();
			payload.LoadXml(response);

			Assert.That(payload.HasChildNodes);
			Assert.That(payload.SelectSingleNode("//serverTime"), Is.Not.Null);
		}

		[Test]
		public void Should_use_api_uri_provided_by_IApiUri_interface()
		{
			const string ExpectedApiUri = "http://api.7dizzle";

			Given_a_urlresolver_that_returns_valid_xml();

			var apiUri = A.Fake<IApiUri>();

			A.CallTo(() => apiUri.Uri).Returns(ExpectedApiUri);

			IOAuthCredentials oAuthCredentials = EssentialDependencyCheck<IOAuthCredentials>.Instance;
			var endpointResolver = new RequestCoordinator(_httpClient, _urlSigner, oAuthCredentials, apiUri);

			var endPointState = new RequestData
			{
				UriPath = "test",
				HttpMethod = HttpMethod.Get,
				Headers = new Dictionary<string, string>()
			};

			endpointResolver.GetDataAsync(endPointState);

			A.CallTo(() => apiUri.Uri).MustHaveHappened(Repeated.Exactly.Once);

			_httpClient.GetAsyncOnUrlContainingMustHaveHappenedOnce(ExpectedApiUri);
		}

		[Test]
		public void Construct_url_should_combine_url_and_query_params_for_get_requests()
		{
			const string UriPath = "something";
			var expectedResult = ApiUrl + "/" + UriPath + "?oauth_consumer_key=" + _consumerKey;
			var result = _requestCoordinator.EndpointUrl(new RequestData { UriPath = UriPath });

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void Construct_url_should_combine_url_and_not_query_params_for_post_requests()
		{
			const string UriPath = "something";
			var request = new RequestData
			{
				UriPath = UriPath,
				HttpMethod = HttpMethod.Post
			};
			var result = _requestCoordinator.EndpointUrl(request);

			Assert.That(result, Is.EqualTo(ApiUrl + "/" + UriPath));
		}

		private void Given_a_urlresolver_that_returns_valid_xml()
		{
			_httpClient.MockGetAsync(new Response(HttpStatusCode.OK, ServiceStatus));
		}
	}
}