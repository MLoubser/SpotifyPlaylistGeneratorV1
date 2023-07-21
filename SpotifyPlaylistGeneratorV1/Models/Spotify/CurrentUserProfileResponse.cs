namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{

    //Not using all the fields!
    public class CurrentUserProfileResponseModel
    {
        public string country { get; set; } = default!;
        public string display_name { get; set; } = default!;
        public string email { get; set; } = default!;
        public string id { get; set; } = default!;
    }
}
