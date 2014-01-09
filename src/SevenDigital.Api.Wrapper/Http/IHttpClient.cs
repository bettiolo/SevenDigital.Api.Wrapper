using System.Threading.Tasks;

namespace SevenDigital.Api.Wrapper.Http
{
	public interface IHttpClient
	{
		Task<Response> GetAsync(GetRequest request);
		Task<Response> PostAsync(PostRequest request);
	}
}