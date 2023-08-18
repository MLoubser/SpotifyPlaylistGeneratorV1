namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface ISpotify
    {
        bool IsLoggedIn();
        string GenerateSignupLink(string? CSRFToken = null);
        public Task<bool> GetAccessTokens(string accessCode);
        Task<string?> CreateNewPlaylistAsync(string PlaylistName, string? PlaylistDescription = null, bool PublicPlaylist = false);
        Task<bool> InitializeComponentAsync(string? userNameWithoutContext = null);
        Task<string?> GetSpotifyInternalIdFromDescriptionAsync(string trackDescription);
        Task<bool> AddTracksToPlaylistAsync(string playlistId, List<string> ItemIds);
    }
}
