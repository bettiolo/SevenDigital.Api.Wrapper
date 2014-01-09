using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using FakeItEasy;
using NUnit.Framework;
using SevenDigital.Api.Schema;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.Exceptions;
using SevenDigital.Api.Wrapper.Unit.Tests.Http;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests
{
	[TestFixture]
	public class FluentApiTests
	{
		private const string ValidStatusXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><response status=\"ok\" version=\"1.2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"http://api.7digital.com/1.2/static/7digitalAPI.xsd\"><serviceStatus><serverTime>2011-05-31T15:31:22Z</serverTime></serviceStatus></response>";

		private readonly Response _stubResponse = new Response(HttpStatusCode.OK, ValidStatusXml);

		[Test]
		public async void Should_fire_requestcoordinator_with_correct_endpoint_on_resolve()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			requestCoordinator.MockGetDataAsync(_stubResponse);

			await new FluentApi<Status>(requestCoordinator).PleaseAsync();

			Expression<Func<Task<Response>>> callWithEndpointStatus =
				() => requestCoordinator.GetDataAsync(A<RequestData>.That.Matches(x => x.Endpoint == "status"));

			A.CallTo(callWithEndpointStatus).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Test]
		public async void Should_fire_requestcoordinator_with_correct_methodname_on_resolve()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			requestCoordinator.MockGetDataAsync(_stubResponse);

			await new FluentApi<Status>(requestCoordinator).WithMethod(HttpMethod.Post).PleaseAsync();

			Expression<Func<Task<Response>>> callWithMethodPost =
				() => requestCoordinator.GetDataAsync(A<RequestData>.That.Matches(x => x.HttpMethod == HttpMethod.Post));

			A.CallTo(callWithMethodPost).MustHaveHappened(Repeated.Exactly.Once);
		}

		[Test]
		public async void Should_fire_requestcoordinator_with_correct_parameters_on_resolve()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			requestCoordinator.MockGetDataAsync(_stubResponse);

			await new FluentApi<Status>(requestCoordinator).WithParameter("artistId", "123").PleaseAsync();

			Expression<Func<Task<Response>>> callWithArtistId123 =
				() => requestCoordinator.GetDataAsync(A<RequestData>.That.Matches(x => x.Parameters["artistId"] == "123"));

			A.CallTo(callWithArtistId123).MustHaveHappened();

		}
		[Test]
		public void Should_use_custom_http_client()
		{
			var fakeRequestCoordinator = A.Fake<IRequestCoordinator>();
			var fakeHttpClient = new FakeHttpClientWrapper(_stubResponse);

			new FluentApi<Status>(fakeRequestCoordinator).UsingClient(fakeHttpClient);

			Assert.That(fakeRequestCoordinator.HttpClient, Is.EqualTo(fakeHttpClient));
		}

		public void Should_throw_exception_when_null_client_is_used()
		{
			var fakeRequestCoordinator = A.Fake<IRequestCoordinator>();
			var api = new FluentApi<Status>(fakeRequestCoordinator);

			Assert.Throws<ArgumentNullException>(() => api.UsingClient(null));
		}

		[Test]
		public async void should_put_payload_in_action_result()
		{
			var requestCoordinator = new FakeRequestCoordinator(_stubResponse);

			var status = await new FluentApi<Status>(requestCoordinator)
				.PleaseAsync();

			Assert.That(status, Is.Not.Null);
		}

		[Test]
		public async void Should_wrap_webexception_under_api_exception_to_be_able_to_know_the_URL()
		{
			const string url = "http://foo.bar.baz/status";

			var requestCoordinator = A.Fake<IRequestCoordinator>();
			requestCoordinator.MockThrowsWebException();
			requestCoordinator.MockUrl(url);

			var api = new FluentApi<Status>(requestCoordinator);
			ApiWebException ex = null;
			try
			{
				await api.PleaseAsync();

			}
			catch (ApiWebException actualEx)
			{
				ex = actualEx;
			}

			Assert.That(ex.InnerException, Is.Not.Null);
			Assert.That(ex.Uri, Is.EqualTo(url));
			Assert.That(ex.InnerException.GetType(), Is.EqualTo(typeof(WebException)));
		}

		[Test]
		public void Should_throw_exception_when_null_cache_is_used()
		{
			var fakeRequestCoordinator = A.Fake<IRequestCoordinator>();
			var api = new FluentApi<Status>(fakeRequestCoordinator);

			Assert.Throws<ArgumentNullException>(() => api.UsingCache(null));
		}

		[Test]
		public void Should_read_cache()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			var cache = new FakeCache();
			requestCoordinator.MockGetDataAsync(_stubResponse);

			new FluentApi<Status>(requestCoordinator).UsingCache(cache).PleaseAsync();

			Assert.That(cache.TryGetCount, Is.EqualTo(1));
		}

		[Test]
		public async void Should_write_to_cache_on_success()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			var cache = new FakeCache();
			requestCoordinator.MockGetDataAsync(_stubResponse);

			await new FluentApi<Status>(requestCoordinator).UsingCache(cache).PleaseAsync();

			Assert.That(cache.SetCount, Is.EqualTo(1));
			Assert.That(cache.CachedResponses.Count, Is.EqualTo(1));
			Assert.That(cache.CachedResponses[0], Is.EqualTo(_stubResponse));
		}

		[Test]
		public async void Should_return_value_from_cache()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			var cache = new FakeCache();
			cache.StubResponse = _stubResponse;

			var fluentApi = new FluentApi<Status>(requestCoordinator).UsingCache(cache);
			var status = await fluentApi.PleaseAsync();

			Assert.That(cache.TryGetCount, Is.EqualTo(1));
			Assert.That(status, Is.Not.Null);
		}

		[Test]
		public async void Should_not_hit_endpoint_when_value_is_found_in_cache()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			var cache = new FakeCache();
			cache.StubResponse = _stubResponse;

			var fluentApi = new FluentApi<Status>(requestCoordinator).UsingCache(cache);
			await fluentApi.PleaseAsync();
			A.CallTo(() => requestCoordinator.GetDataAsync(A<RequestData>.Ignored)).MustNotHaveHappened();
		}

		[Test]
		public async void Should_not_write_to_cache_on_failure()
		{
			var requestCoordinator = A.Fake<IRequestCoordinator>();
			var cache = new FakeCache();
			requestCoordinator.MockThrowsWebException();
			requestCoordinator.MockUrl("http://foo.com/bar");

			var api = new FluentApi<Status>(requestCoordinator).UsingCache(cache);

			ApiWebException ex = null;
			try
			{
				await api.PleaseAsync();

			}
			catch (ApiWebException actualEx)
			{
				ex = actualEx;
			}

			Assert.That(ex, Is.Not.Null);
			Assert.That(cache.SetCount, Is.EqualTo(0));
			Assert.That(cache.CachedResponses.Count, Is.EqualTo(0));
		}
	}

	internal class FakeCache : IResponseCache
	{
		public int SetCount { get; set; }
		public int TryGetCount { get; set; }
		public IList<Response> CachedResponses { get; set; }

		public Response StubResponse { get; set; }

		internal FakeCache()
		{
			CachedResponses = new List<Response>();
		}

		public void Set(RequestData key, Response value)
		{
			SetCount++;
			CachedResponses.Add(value);
		}

		public bool TryGet(RequestData key, out Response value)
		{
			TryGetCount++;
			value = StubResponse;
			return (StubResponse != null);
		}
	}
}
