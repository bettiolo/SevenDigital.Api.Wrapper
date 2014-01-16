using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SevenDigital.Api.Wrapper.Http
{
	public class HttpClientMediator : IHttpClient
	{
		public async Task<Response> GetAsync(GetRequest request)
		{
			var httpClient = MakeHttpClient(request.Headers);
			using (var httpResponseMessage = await httpClient.GetAsync(request.Url))
			{
				return await MakeResponse(httpResponseMessage);
			}
		}

		public async Task<Response> PostAsync(PostRequest request)
		{
			var httpClient = MakeHttpClient(request.Headers);
			var content = PostParamsToHttpContent(request.Body);

			using (var httpResponseMessage = await httpClient.PostAsync(request.Url, content))
			{
				return await MakeResponse(httpResponseMessage);
			}
		}

		private static HttpContent PostParamsToHttpContent(string body)
		{
			HttpContent content = new StringContent(body);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			return content;
		}

		private HttpClient MakeHttpClient(IDictionary<string, string> headers)
		{
			var httpClient = new HttpClient(new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.GZip
				});

			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 1.0));
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