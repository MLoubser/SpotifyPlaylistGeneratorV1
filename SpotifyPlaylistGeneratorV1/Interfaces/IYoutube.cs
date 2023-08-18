namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IYoutube
    {
        Task<string?> FetchVideoDescriptionByIdAsync(string videoId);
        List<string> ProcessDescriptionForTrackList(string? videoDescription, bool artistFirst = true);
    }
}
