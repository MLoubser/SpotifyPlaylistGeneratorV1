namespace SpotifyPlaylistGeneratorV1.Models.BackgroundService
{
    public class PlaylistRequestItem
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = string.Empty;
        public string YouTubeUrl { get; set; } = string.Empty;
        public string PlaylistName { get; set; } = "SPG Playlist";

    }
}
