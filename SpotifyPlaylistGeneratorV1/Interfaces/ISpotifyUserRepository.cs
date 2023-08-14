using SpotifyPlaylistGeneratorV1.Models.Database;

namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface ISpotifyUserRepository
    {
        public Task<SpotifyUserModel?> FetchUserFromUsernameAsync(string username);
        public Task<bool> AddUserAsync(SpotifyUserModel user);
        public Task<bool> ClearUserTokensAsync(string username);
        public Task<bool> UpdateUserAsync(SpotifyUserModel user);
    }
}
