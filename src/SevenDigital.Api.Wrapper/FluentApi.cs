using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Environment;
using SevenDigital.Api.Wrapper.Exceptions;
using SevenDigital.Api.Wrapper.Http;
using SevenDigital.Api.Wrapper.Requests;
using SevenDigital.Api.Wrapper.Requests.Serializing;
using SevenDigital.Api.Wrapper.Responses;
using SevenDigital.Api.Wrapper.Responses.Parsing;

namespace SevenDigital.Api.Wrapper
{
	public class FluentApi<T> : IFluentApi<T> where T : class
	{
		private IHttpClient _httpClient;
		private readonly IRequestBuilder _requestBuilder;

		private readonly RequestData _requestData;
		private readonly IResponseParser<T> _parser;
		private IResponseCache _responseCache = new NullResponseCache();
		private readonly List<IPayloadSerializer> _payloadSerializers= new List<IPayloadSerializer>
		{
			new XmlPayloadSerializer(),
			new JsonPayloadSerializer()
		};

		public FluentApi(IHttpClient httpClient, IRequestBuilder requestBuilder) 
		{
			_httpClient = httpClient;
			_requestBuilder = requestBuilder;

			var attributeValidation = new AttributeRequestDataBuilder<T>();
			_requestData = attributeValidation.BuildRequestData();

			_parser = new ResponseParser<T>(new ApiResponseDetector());
		}

		public FluentApi(IRequestBuilder requestBuilder) : this(new HttpClientMediator(), requestBuilder)
		{}

		public FluentApi(IOAuthCredentials oAuthCredentials, IApiUri apiUri)
			: this(new HttpClientMediator(), new RequestBuilder(apiUri, oAuthCredentials))
			{}

		public FluentApi()
			: this(new HttpClientMediator(), new RequestBuilder(EssentialDependencyCheck<IApiUri>.Instance, EssentialDependencyCheck<IOAuthCredentials>.Instance))
		{}

		public IFluentApi<T> UsingClient(IHttpClient httpClient)
		{
			if (httpClient == null)
			{
				throw new ArgumentNullException("httpClient");
			}

			_httpClient = httpClient;
			return this;
		}

		public IFluentApi<T> UsingCache(IResponseCache responseCache)
		{
			if (responseCache == null)
			{
				throw new ArgumentNullException("responseCache");
			}

			_responseCache = responseCache;
			return this;
		}

		public virtual IFluentApi<T> WithMethod(string methodName)
		{
			_requestData.HttpMethod =  HttpMethodHelpers.Parse(methodName);
			return this;
		}

		public virtual IFluentApi<T> WithParameter(string parameterName, string parameterValue)
		{
			_requestData.Parameters[parameterName] = parameterValue;
			return this;
		}

		public virtual IFluentApi<T> ClearParameters()
		{
			_requestData.Parameters.Clear();
			return this;
		}

		public virtual IFluentApi<T> ForUser(string token, string secret)
		{
			_requestData.UserToken = token;
			_requestData.TokenSecret = secret;
			return this;
		}

		public IFluentApi<T> WithPayload(string contentType, string payload)
		{
			_requestData.Payload = new RequestPayload(contentType, payload);
			return this;
		}

		public IFluentApi<T> WithPayload<TPayload>(TPayload payload) where TPayload : class
		{
			return WithPayload(payload, PayloadFormat.Xml);
		}

		public IFluentApi<T> WithPayload<TPayload>(TPayload payload, PayloadFormat payloadFormat) where TPayload : class
		{
			var correctSerializer = _payloadSerializers.FirstOrDefault(payloadSerializer => payloadSerializer.Handles == payloadFormat);

			_requestData.Payload = new RequestPayload(correctSerializer.ContentType, correctSerializer.Serialize(payload));
			return this;
		}

		public async Task<Response> Response()
		{
			var request = _requestBuilder.BuildRequest(_requestData);

			try
			{
				return await _httpClient.Send(request);
			}
			catch (WebException webException)
			{
				throw new ApiWebException(webException.Message, webException, request);
			}
		}

		public virtual async Task<T> Please()
		{
			Response response;

			var foundInCache = _responseCache.TryGet(_requestData, out response);
			if (!foundInCache)
			{
				response = await Response();
			}

			var result = _parser.Parse(response);

				// set to cache only after all validation and parsing has succeeded
			if (!foundInCache)
			{
				_responseCache.Set(_requestData, response);
			}
			return result;
		}

		public virtual string EndpointUrl
		{
			get
			{
				var request = _requestBuilder.BuildRequest(_requestData);
				return request.Url;
			}
		}

		public IDictionary<string, string> Parameters
		{
			get { return _requestData.Parameters; }
		}
	}
}