using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.EndpointResolution.OAuth;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.EndpointResolution
{
	public class RequestCoordinator : IRequestCoordinator
	{
		private IHttpClient _httpClient;
		private readonly IUrlSigner _urlSigner;
		private readonly IOAuthCredentials _oAuthCredentials;
		private readonly IApiUri _apiUri;

		public RequestCoordinator(IHttpClient httpClient, IUrlSigner urlSigner, IOAuthCredentials oAuthCredentials, IApiUri apiUri)
		{
			_httpClient = httpClient;
			_urlSigner = urlSigner;
			_oAuthCredentials = oAuthCredentials;
			_apiUri = apiUri;
		}

		public IHttpClient HttpClient
		{
			get { return _httpClient; }
			set { _httpClient = value; }
		}

		public Task<Response> GetDataAsync(RequestData requestData)
		{
			if (requestData.HttpMethod == HttpMethod.Get)
			{
				return HitGetEndpoint(requestData);
			}
			if (requestData.HttpMethod == HttpMethod.Post)
			{
				return HitPostEndpoint(requestData);
			}

			throw new ArgumentException("Unimplemented http httpMethod " + requestData.HttpMethod);
		}

		public async Task<Response> HitGetEndpoint(RequestData requestData)
		{
			var url = EndpointUrl(requestData);
			var signedUrl = SignHttpGetUrl(url, requestData);

			return await _httpClient.GetAsync(requestData.Headers, signedUrl);
		}

		public async Task<Response> HitPostEndpoint(RequestData requestData)
		{
			var url = EndpointUrl(requestData);
			var signedParams = SignHttpPostParams(url, requestData);

			return await _httpClient.PostAsync(requestData.Headers, signedParams, url);
		}

		private string SignHttpGetUrl(string uri, RequestData requestData)
		{
			if (requestData.IsSigned)
			{
				return _urlSigner.SignGetUrl(uri, requestData.UserToken, requestData.TokenSecret, _oAuthCredentials);
			}
			return uri;
		}

		private IDictionary<string, string> SignHttpPostParams(string uri, RequestData requestData)
		{
			if (requestData.IsSigned)
			{
				return _urlSigner.SignPostRequest(uri, requestData.UserToken, requestData.TokenSecret, _oAuthCredentials, requestData.Parameters);
			}
			return requestData.Parameters;
		}

		public virtual string EndpointUrl(RequestData requestData)
		{
			var apiUri = requestData.UseHttps ? _apiUri.SecureUri : _apiUri.Uri;

			var mutableParams = new Dictionary<string, string>(requestData.Parameters);
			var route = SubstituteRouteParameters(requestData.UriPath, mutableParams);
			var uriString = apiUri + "/" + route;

			if (requestData.HttpMethod == HttpMethod.Get)
			{
				var oauthParam = "oauth_consumer_key=" + _oAuthCredentials.ConsumerKey;
				uriString = uriString + "?" + oauthParam;

				var otherQueryParams = mutableParams.ToQueryString(true);
				if (!string.IsNullOrEmpty(otherQueryParams))
				{
					uriString = uriString + "&" + otherQueryParams;
				}
			}
			return uriString;
		}

		private static string SubstituteRouteParameters(string endpointUri, IDictionary<string, string> parameters)
		{
			var regex = new Regex("{(.*?)}");
			var result = regex.Matches(endpointUri);
			foreach (var match in result)
			{
				var key = match.ToString().Remove(match.ToString().Length - 1).Remove(0, 1);
				var entry = parameters.First(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
				parameters.Remove(entry.Key);
				endpointUri = endpointUri.Replace(match.ToString(), entry.Value);
			}

			return endpointUri.ToLower();
		}
	}
}