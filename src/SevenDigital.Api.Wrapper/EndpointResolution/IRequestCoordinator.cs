using System.Threading.Tasks;
using SevenDigital.Api.Wrapper.Http;

namespace SevenDigital.Api.Wrapper.EndpointResolution
{
	public interface IRequestCoordinator
	{
		Task<Response> GetDataAsync(RequestData requestData);
		string EndpointUrl(RequestData requestData);

		IHttpClient HttpClient { get; set; }
	}
}