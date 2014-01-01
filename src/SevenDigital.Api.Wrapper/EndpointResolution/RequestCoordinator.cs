using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.EndpointResolution.RequestHandlers;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.EndpointResolution
{
	public class RequestCoordinator : IRequestCoordinator
	{
		private readonly IEnumerable<RequestHandler> _requestHandlers;

		public IHttpClient HttpClient { get; set; }

		public RequestCoordinator(IHttpClient httpClient, IEnumerable<RequestHandler> requestHandlers)
		{
			HttpClient = httpClient;
			_requestHandlers = requestHandlers;
		}

        public string EndpointUrl(RequestData requestData)
        {
			return ConstructBuilder(requestData).ConstructEndpoint(requestData);
		}

		private RequestHandler ConstructBuilder(RequestData requestData)
		{
			foreach (var requestHandler in _requestHandlers)
			{
                if (requestHandler.HandlesMethod(requestData.HttpMethod))
				{
					return requestHandler;
				}
			}
			throw new NotImplementedException("No RequestHandlers supplied that can deal with this method");
		}

        public Task<Response> GetDataAsync(RequestData requestData)
        {
            var builder = ConstructBuilder(requestData);
            builder.HttpClient = HttpClient;
            return builder.HitEndpoint(requestData);
        }
    }
}