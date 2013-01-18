using System.Collections.Generic;
using System.Threading.Tasks;

namespace SevenDigital.Api.Wrapper.Http
{
	public interface IHttpClient
	{
		Task<Response> GetAsync(IDictionary<string, string> headers, string url);
		Task<Response> PostAsync(IDictionary<string, string> headers, IDictionary<string, string> postParams, string url);
	}
}