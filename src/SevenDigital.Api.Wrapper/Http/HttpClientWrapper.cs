using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.EndpointResolution;

namespace SevenDigital.Api.Wrapper.Http
{
	public class HttpClientWrapper : IHttpClient
	{
		public async Task<Response> GetAsync(IDictionary<string, string> headers, string url)
		{
			var httpClient = MakeHttpClient(headers);
			var httpResponseMessage = await httpClient.GetAsync(url);
			return await MakeResponse(httpResponseMessage);
		}

		public async Task<Response> PostAsync(IDictionary<string, string> headers, IDictionary<string, string> postParams, string url)
		{
			var httpClient = MakeHttpClient(headers);
			var content = PostParamsToHttpContent(postParams);

			var httpResponseMessage = await httpClient.PostAsync(url, content);
			return await MakeResponse(httpResponseMessage);
		}

		private static HttpContent PostParamsToHttpContent(IDictionary<string, string> postParams)
		{
			string postData = postParams.ToQueryString();
			HttpContent content = new StringContent(postData);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			return content;
		}

		private HttpClient MakeHttpClient(IDictionary<string, string> headers)
		{
			var httpClient = new HttpClient(new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip
			});

			httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 1.0));
			httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("UTF8", 0.9));

			httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("7digital-.Net-Api-Wrapper", "4.5"));

			foreach (var header in headers)
			{
				httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
			}

			return httpClient;
		}

		private static async Task<Response> MakeResponse(HttpResponseMessage httpResponse)
		{
			var headers = HttpHelpers.MapHeaders(httpResponse.Headers);
			string responseBody = await httpResponse.Content.ReadAsStringAsync();
			return new Response(httpResponse.StatusCode, headers, responseBody);
		}
	}
}