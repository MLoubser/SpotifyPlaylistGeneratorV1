using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using SpotifyPlaylistGeneratorV1.Interfaces;
using System.Text.RegularExpressions;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class YoutubeV1 : IYoutube
    {


        private const string newRowMatchString = @"\d{1,3}(:\d)+\s*.+-.+[\r\n]+";
        private const string newItemMatchString = @"[^\d :]+.+-.+";
        private const int maxTrackItems = 100;
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

        public List<string> ProcessDescriptionForTrackList(string? videoDescription, bool artistFirst = true)
        {
            var TempList = new List<string>();

            if (videoDescription != null)
            {   
                foreach (Match match in Regex.Matches(videoDescription, newRowMatchString,RegexOptions.ECMAScript, TimeSpan.FromMinutes(1)))
                {
                    if(TempList.Count < maxTrackItems)
                    {
                        var itemMatch = Regex.Match(match.Value.Trim(new char[] { '\r', '\n', ' ' }), newItemMatchString);
            
                        if (itemMatch != null && itemMatch.Success)
                        {
                            string itemToAdd = "";

                            if (artistFirst)
                            {
                                var splitItem = itemMatch.Value.Split(" - ");

                                if(splitItem.Length == 2)
                                {
                                    //Take only first artist
                                    var artistList = splitItem[0].Split(',');
                                    var artist = splitItem[0];
                                    if(artistList.Length > 1)
                                    {
                                        artist = artistList[0];
                                    }

                                    itemToAdd =  splitItem[1] + " - " + artist;
                                }
                                else
                                {
                                    itemToAdd = itemMatch.Value;
                                }
                            }
                            else
                            {
                                itemToAdd = itemMatch.Value;
                            }

                            if(!TempList.Contains(itemToAdd))
                            {
                                TempList.Add(itemToAdd);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                var test2 = "";
            }

            return TempList;
        }
    }
}
