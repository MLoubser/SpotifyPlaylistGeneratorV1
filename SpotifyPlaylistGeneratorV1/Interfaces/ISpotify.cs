namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface ISpotify
    {
        public bool IsLoggedIn();
        public string GenerateSignupLink(string? CSRFToken = null);
        public Task<bool> GetAccessTokens(string accessCode);
        public Task<bool> CreateNewPlaylist(string PlaylistName, string? PlaylistDescription = null, bool PublicPlaylist = false);
        public Task<bool> InitializeComponentAsync(string? userNameWithoutContext = null);
    }
}
