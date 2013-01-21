using System;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.EndpointResolution;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.Unit.Tests.Http
{
	public class FakeRequestCoordinator : IRequestCoordinator
	{
		private readonly Response _fakeResponse;

		public FakeRequestCoordinator(Response fakeResponse)
		{
			_fakeResponse = fakeResponse;
		}

		public async Task<Response> GetDataAsync(RequestData requestData)
		{
			return await Task.FromResult(_fakeResponse);
		}

		public string EndpointUrl(RequestData requestData)
		{
			throw new NotImplementedException();
		}

		public IHttpClient HttpClient
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
	}
}