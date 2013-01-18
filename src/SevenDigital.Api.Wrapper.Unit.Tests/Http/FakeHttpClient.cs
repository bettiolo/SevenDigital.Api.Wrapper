using System.Collections.Generic;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.Http
{
	public class FakeHttpClientWrapper : IHttpClient
	{
		private readonly Response _fakeResponse;

		public FakeHttpClientWrapper(Response fakeResponse)
		{
			_fakeResponse = fakeResponse;
		}

		public async Task<Response> GetAsync(IDictionary<string, string> headers, string url)
		{
			return await Task.FromResult(_fakeResponse);
		}

		public async Task<Response> PostAsync(IDictionary<string, string> headers, IDictionary<string, string> postParams, string url)
		{
			return await Task.FromResult(_fakeResponse);
		}
	}
}