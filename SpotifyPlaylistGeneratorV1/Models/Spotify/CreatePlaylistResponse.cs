namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{

    //Not all fields!!
    public class CreatePlaylistResponseModel
    {
        public bool collaborative { get; set; } = default!;
        public string description { get; set; } = default!;
        public bool @public { get; set; } = default!;
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
    }
}
