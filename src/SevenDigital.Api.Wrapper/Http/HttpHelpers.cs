using System.Collections.Generic;
using System.Net.Http.Headers;

namespace SevenDigital.Api.Wrapper.Http
{
	public static class HttpHelpers
	{
		public static IDictionary<string, string> MapHeaders(HttpHeaders headerCollection)
		{
			var resultHeaders = new Dictionary<string, string>();

			foreach (var header in headerCollection)
			{
				resultHeaders.Add(header.Key, string.Join(",", header.Value));
			}

			return resultHeaders;
		}
	}
}