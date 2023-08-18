using SpotifyPlaylistGeneratorV1.Models.Database;

namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface ISpotifyUserRepository
    {
        Task<SpotifyUserModel?> FetchUserFromUsernameAsync(string username);
        Task<bool> AddUserAsync(SpotifyUserModel user);
        Task<bool> ClearUserTokensAsync(string username);
        Task<bool> UpdateUserAsync(SpotifyUserModel user);
    }
}
