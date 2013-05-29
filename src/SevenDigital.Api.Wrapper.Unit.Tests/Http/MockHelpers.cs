using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.Http
{
	public static class MockHelpers
	{
		public static void MockGetAsync(this IHttpClient httpClient, Response response)
		{
			A.CallTo(() => httpClient.GetAsync(A<IDictionary<string, string>>.Ignored, A<string>.Ignored))
				.ReturnsLazily(() => Task.FromResult(response));
		}

		public static void GetAsyncOnUrlMustHaveHappened(this IHttpClient httpClient, string expected)
		{
			A.CallTo(() => httpClient.GetAsync(
				A<IDictionary<string, string>>.Ignored,
				A<string>.That.Matches(y => y == expected)))
				.MustHaveHappened();
		}

		public static void GetAsyncOnUrlMustHaveHappenedOnce(this IHttpClient httpClient, string expected)
		{
			A.CallTo(() => httpClient.GetAsync(
				A<IDictionary<string, string>>.Ignored,
				A<string>.That.Matches(y => y == expected)))
				.MustHaveHappened(Repeated.Exactly.Once);
		}

		public static void GetAsyncOnUrlContainingMustHaveHappenedOnce(this IHttpClient httpClient, string expected)
		{
			A.CallTo(() => httpClient.GetAsync(
				A<IDictionary<string, string>>.Ignored,
				A<string>.That.Matches(y => y.Contains(expected))))
				.MustHaveHappened(Repeated.Exactly.Once);
		}

		public static void MockGetDataAsync(this IRequestCoordinator requestCoordinator, Response response)
		{
			A.CallTo(() => requestCoordinator.GetDataAsync(A<RequestData>.Ignored))
				.ReturnsLazily(() => Task.FromResult(response));
		}

		public static void MockThrowsWebException(this IRequestCoordinator requestCoordinator)
		{
			A.CallTo(() => requestCoordinator.GetDataAsync(A<RequestData>.Ignored)).Throws<WebException>();
		}

		public static void MockUrl(this IRequestCoordinator requestCoordinator, string url)
		{
			A.CallTo(() => requestCoordinator.EndpointUrl(A<RequestData>.Ignored)).Returns(url);
		}
	}
}