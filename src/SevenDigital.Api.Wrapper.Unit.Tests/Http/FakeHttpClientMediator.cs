using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.Http
{
	public class FakeHttpClientMediator : IHttpClient
	{
		private readonly Response _fakeResponse;

		public FakeHttpClientMediator(Response fakeResponse)
		{
			_fakeResponse = fakeResponse;
		}

		public async Task<Response> GetAsync(GetRequest request)
		{
			return await Task.FromResult(_fakeResponse);
		}

		public async Task<Response> PostAsync(PostRequest request)
		{
			return await Task.FromResult(_fakeResponse);
		}
	}
}