using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SevenDigital.Api.Schema.OAuth;
using SevenDigital.Api.Wrapper.EndpointResolution.OAuth;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.EndpointResolution.RequestHandlers
{
	public class PostRequestHandler : RequestHandler
	{
		private readonly IOAuthCredentials _oAuthCredentials;
		private readonly ISignatureGenerator _signatureGenerator;

		public PostRequestHandler(IApiUri apiUri, IOAuthCredentials oAuthCredentials, ISignatureGenerator signatureGenerator)
			: base(apiUri)
		{
			_oAuthCredentials = oAuthCredentials;
			_signatureGenerator = signatureGenerator;
		}

		public override bool HandlesMethod(HttpMethod method)
		{
			return method == HttpMethod.Post;
		}

		public override async Task<Response> HitEndpoint(RequestData requestData)
		{
            var uri = ConstructEndpoint(requestData);
            var signedParams = SignHttpPostParams(uri, requestData);

            return await HttpClient.PostAsync(requestData.Headers, signedParams, uri);
		}


		private IDictionary<string, string> SignHttpPostParams(string uri, RequestData requestData)
		{
			if (!requestData.IsSigned)
			{
				return requestData.Parameters;
			}
			var oAuthSignatureInfo = new OAuthSignatureInfo
			{
				FullUrlToSign = uri,
				ConsumerCredentials = _oAuthCredentials,
				HttpMethod = "POST",
				UserAccessToken = new OAuthAccessToken { Token = requestData.UserToken, Secret = requestData.TokenSecret },
				PostData = requestData.Parameters
			};

			return _signatureGenerator.SignWithPostData(oAuthSignatureInfo);
		}

		protected override string AdditionalParameters(Dictionary<string, string> newDictionary)
		{
			return String.Empty;
		}
	}
}