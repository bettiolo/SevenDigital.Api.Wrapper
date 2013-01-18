using System.Collections.Generic;
using System.Net;
using FakeItEasy;
using NUnit.Framework;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.EndpointResolution.OAuth;
using SevenDigital.Api.Wrapper.Http;
using SevenDigital.Api.Wrapper.Unit.Tests.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.EndpointResolution
{
	[TestFixture]
	public class EndpointResolverTests
	{
		private IRequestCoordinator _requestCoordinator;

		[SetUp]
		public void Setup()
		{
			var response = new Response(HttpStatusCode.OK, "body");
			var httpClient = A.Fake<IHttpClient>();
			httpClient.MockGetAsync(response);

			var apiUri = A.Fake<IApiUri>();
			A.CallTo(() => apiUri.Uri)
				.Returns("http://uri/");

			_requestCoordinator = new RequestCoordinator(httpClient, new UrlSigner(), new AppSettingsCredentials(), apiUri);
		}

		[Test]
		public void Should_substitue_route_parameters()
		{
			var endpointInfo = new RequestData
			{
				UriPath = "something/{route}",
				Parameters = new Dictionary<string, string>
						{
							{"route","routevalue"}
						}
			};

			var result = _requestCoordinator.EndpointUrl(endpointInfo);

			Assert.That(result, Is.StringContaining("something/routevalue"));
		}

		[Test]
		public void Should_substitue_multiple_route_parameters()
		{
			var endpointInfo = new RequestData
			{
				UriPath = "something/{firstRoute}/{secondRoute}/thrid/{thirdRoute}",
				Parameters = new Dictionary<string, string>
					{
						{"firstRoute" , "firstValue"},
						{"secondRoute","secondValue"},
						{"thirdRoute" , "thirdValue"}
					}
			};

			var result = _requestCoordinator.EndpointUrl(endpointInfo);

			Assert.That(result, Is.StringContaining("something/firstvalue/secondvalue/thrid/thirdvalue"));
		}

		[Test]
		public void Routes_should_be_case_insensitive()
		{
			var endpointInfo = new RequestData
			{
				UriPath = "something/{firstRoUte}/{secOndrouTe}/thrid/{tHirdRoute}",
				Parameters = new Dictionary<string, string>
					{
						{"firstRoute" , "firstValue"},
						{"secondRoute","secondValue"},
						{"thirdRoute" , "thirdValue"}
							
					}
			};

			var result = _requestCoordinator.EndpointUrl(endpointInfo);

			Assert.That(result, Is.StringContaining("something/firstvalue/secondvalue/thrid/thirdvalue"));
		}

		[Test]
		public void Should_handle_dashes_and_numbers()
		{
			var endpointInfo = new RequestData
			{
				UriPath = "something/{route-66}",
				Parameters = new Dictionary<string, string>
					{
						{"route-66","routevalue"}
					}
			};

			var result = _requestCoordinator.EndpointUrl(endpointInfo);

			Assert.That(result, Is.StringContaining("something/routevalue"));
		}

		[Test]
		public void Should_remove_parameters_that_match()
		{
			var endpointInfo = new RequestData
			{
				UriPath = "something/{route-66}",
				Parameters = new Dictionary<string, string>
					{
						{"route-66","routevalue"}
					}
			};

			var result = _requestCoordinator.EndpointUrl(endpointInfo);

			Assert.That(result, Is.Not.StringContaining("route-66=routevalue"));
		}
	}
}