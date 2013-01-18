using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SevenDigital.Api.Wrapper.Http
{
	[Serializable]
	public class RequestData
	{
		public string UriPath { get; set; }

		public HttpMethod HttpMethod { get; set; }

		public Dictionary<string, string> Parameters { get; set; }

		public Dictionary<string, string> Headers { get; set; }

		public bool UseHttps { get; set; }

		public string UserToken { get; set; }

		public string TokenSecret { get; set; }

		public bool IsSigned { get; set; }

		public RequestData()
		{
			UriPath = string.Empty;
			HttpMethod = HttpMethod.Get;
			Parameters = new Dictionary<string, string>();
			Headers = new Dictionary<string, string>();
			UseHttps = false;
		}
	}
}