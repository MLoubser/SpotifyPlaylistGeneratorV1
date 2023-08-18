namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{
    public class AddToPlaylistRequestModel
    {
        public List<string> uris { get; set; } = default!;
        public int position { get; set; } = 0;
    }
}
