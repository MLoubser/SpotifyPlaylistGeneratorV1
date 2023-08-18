using SpotifyPlaylistGeneratorV1.Models.BackgroundService;

namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IPlaylistCreateQueue
    {
        Task<bool> EnqueueItemAsync(PlaylistRequestItem item);
        Task<PlaylistRequestItem?> DequeueItemAsync(CancellationToken cancellationToken);
    }
}
