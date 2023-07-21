namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{
    public class CreatePlaylistRequestModel
    {
        public string name { get; set; } = default!;
        public bool @public { get; set; } = false;
        public string description { get; set; } = default!;
    }
}
