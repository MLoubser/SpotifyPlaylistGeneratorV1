namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{
    public class SearchRequestResponseModel
    {
        public TrackObjectModel tracks { get; set; } = default!;
    }


    public class TrackObjectModel
    {
        public TrackItemsModel[] items { get; set; } = default!;
    }

    public class TrackItemsModel
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public ArtistObjectModel[] artists { get; set; } = default!;

    }

    public class ArtistObjectModel
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
    }
}
