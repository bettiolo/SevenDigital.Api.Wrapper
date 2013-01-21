using System;
using System.Linq;
using System.Threading.Tasks;
using SevenDigital.Api.Schema.ReleaseEndpoint;
using SevenDigital.Api.Wrapper.Exceptions;
using SevenDigital.Api.Schema.ArtistEndpoint;
using SevenDigital.Api.Schema.LockerEndpoint;

namespace SevenDigital.Api.Wrapper.ExampleUsage 
{
	class Program 
	{
		static void Main(string[] args)
		{
			string s = args[0];
			int artistId = Convert.ToInt32(s);

			var appSettingsCredentials = new AppSettingsCredentials();
			Console.WriteLine("Using creds: {0} - {1}", appSettingsCredentials.ConsumerKey, appSettingsCredentials.ConsumerSecret);

			var task = AsyncWork(artistId);
			task.Wait();

			Console.ReadKey();
		}
		
		private static async Task AsyncWork(int artistId)
		{
			// -- artist/details
			var artist = await Api<Artist>.Create
				.WithArtistId(artistId)
				.PleaseAsync();

			Console.WriteLine("Artist \"{0}\" selected", artist.Name);
			Console.WriteLine("Website url is {0}", artist.Url);
			Console.WriteLine();


			// -- artist/toptracks
			var artistTopTracks = await Api<ArtistTopTracks>
				.Create
				.WithArtistId(artistId)
				.PleaseAsync();

			Console.WriteLine("Top Track: {0}", artistTopTracks.Tracks.FirstOrDefault().Title);
			Console.WriteLine();
			
			// -- artist/browse
			const string searchValue = "Radio";
			var artistBrowse = await Api<ArtistBrowse>
				.Create
				.WithLetter(searchValue)
				.PleaseAsync();

			Console.WriteLine("Browse on \"{0}\" returns: {1}", searchValue, artistBrowse.Artists.FirstOrDefault().Name);
			Console.WriteLine();

			// -- artist/search
			var artistSearch = await Api<ArtistSearch>.Create
				.WithQuery(searchValue)
				.WithPageNumber(1)
				.WithPageSize(10)
				.PleaseAsync();

			Console.WriteLine("Artist Search on \"{0}\" returns: {1} items", searchValue, artistSearch.TotalItems);
			Console.WriteLine();

			// -- release/search
			var releaseSearch = await Api<ReleaseSearch>.Create
				.WithQuery(searchValue)
				.WithPageNumber(1)
				.WithPageSize(10)
				.PleaseAsync();

			Console.WriteLine("Release search on \"{0}\" returns: {1} items", searchValue, releaseSearch.TotalItems);
			Console.WriteLine();

			// -- Debug uri
			string currentUri = Api<ReleaseSearch>.Create.WithQuery("Test").EndpointUrl;
			Console.WriteLine("Release search hits: {0}", currentUri);

			try 
			{
				// -- Deliberate error response
				Console.WriteLine("Trying artist/details without artistId parameter...");
				await Api<Artist>.Create.PleaseAsync();
			} 
			catch (ApiResponseException ex)
			{
				Console.WriteLine("{0} : {1}", ex, ex.Message);
			}

			try 
			{
				// -- Deliberate unauthorized response
				Console.WriteLine("Trying user/locker without any credentials...");
				await Api<Locker>.Create.PleaseAsync();
			} 
			catch (ApiResponseException ex)
			{
				Console.WriteLine("{0} : {1}", ex, ex.Message);
			}

			Console.ReadKey();
		}
	}
}
