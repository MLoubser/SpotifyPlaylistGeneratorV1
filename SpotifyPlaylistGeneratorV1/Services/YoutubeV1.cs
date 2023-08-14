using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class YoutubeV1 : IYoutube
    {
        private readonly YouTubeService _ytService;
        private readonly IConfiguration _config;

        public YoutubeV1(IConfiguration configuration)
        {
            _config = configuration;     
            _ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _config["YoutubeConfig:APIKey"] ?? throw new Exception("YoutubeV1 - Failed to get API key"),
                ApplicationName = _config["YoutubeConfig:AppName"] ?? throw new Exception("YoutubeV1 - Failed to get Application name")
            });
        }

        public async Task<string?> FetchVideoDescriptionByIdAsync(string videoId)
        {
            var Request = _ytService.Videos.List("snippet");

            Request.Id = videoId;

            var Result = (await Request.ExecuteAsync()).Items.FirstOrDefault();

            if(Result != null)
            {
                return Result.Snippet.Description;
            }

            return null;
        }
    }
}
