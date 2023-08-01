using SpotifyPlaylistGeneratorV1.Models.BackgroundService;

namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IPlaylistCreateQueue
    {
        public Task<bool> EnqueueItemAsync(PlaylistRequestItem item);
        public Task<PlaylistRequestItem?> DequeueItemAsync(CancellationToken cancellationToken);
    }
}
