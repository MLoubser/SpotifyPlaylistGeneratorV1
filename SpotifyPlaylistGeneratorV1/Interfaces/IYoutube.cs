namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IYoutube
    {
        public Task<string?> FetchVideoDescriptionByIdAsync(string videoId);
    }
}
