namespace SevenDigital.Api.Wrapper.Responses.Parsing
{
	public interface IApiResponseDetector
	{
		bool IsXml(string responseBody);
		bool IsApiOkResponse(string responseBody);
		bool IsApiErrorResponse(string responseBody);
		bool IsOAuthError(string responseBody);
	}
}