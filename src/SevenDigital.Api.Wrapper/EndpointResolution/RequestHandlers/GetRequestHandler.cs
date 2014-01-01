using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SevenDigital.Api.Schema.OAuth;
using SevenDigital.Api.Wrapper.EndpointResolution.OAuth;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.EndpointResolution.RequestHandlers
{
	public class GetRequestHandler : RequestHandler
	{
		private readonly IOAuthCredentials _oAuthCredentials;
		private readonly ISignatureGenerator _signatureGenerator;

		public GetRequestHandler(IApiUri apiUri, IOAuthCredentials oAuthCredentials, ISignatureGenerator signatureGenerator) : base(apiUri)
		{
			_oAuthCredentials = oAuthCredentials;
			_signatureGenerator = signatureGenerator;
		}

		public override bool HandlesMethod(HttpMethod method)
		{
			return method == HttpMethod.Get;
		}

		public override async Task<Response> HitEndpoint(RequestData requestData)
		{
			var uri = ConstructEndpoint(requestData);
			var signedUrl = SignHttpGetUrl(uri, requestData);

		    return await HttpClient.GetAsync(requestData.Headers, signedUrl);
		}

		private string SignHttpGetUrl(string uri, RequestData requestData)
		{
			if (!requestData.IsSigned)
			{
				return uri;
			}
			
			var oAuthSignatureInfo = new OAuthSignatureInfo
			{
				FullUrlToSign = uri,
				ConsumerCredentials = _oAuthCredentials,
				HttpMethod = "GET",
				UserAccessToken = new OAuthAccessToken { Token = requestData.UserToken, Secret = requestData.TokenSecret }
			};
			return _signatureGenerator.Sign(oAuthSignatureInfo);
		}

		protected override string AdditionalParameters(Dictionary<string, string> newDictionary)
		{
			return string.Format("?oauth_consumer_key={0}&{1}", _oAuthCredentials.ConsumerKey, newDictionary.ToQueryString(true)).TrimEnd('&');
		}
	}
}