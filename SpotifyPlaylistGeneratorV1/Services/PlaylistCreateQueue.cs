using System.Threading.Channels;
using SpotifyPlaylistGeneratorV1.Interfaces;
using SpotifyPlaylistGeneratorV1.Models.BackgroundService;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class PlaylistCreateQueue: IPlaylistCreateQueue
    {
        private readonly Channel<PlaylistRequestItem> _queue;

        public PlaylistCreateQueue(int capacity) 
        {
            var opt = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropWrite
            };

            _queue = Channel.CreateBounded<PlaylistRequestItem>(opt);
        }

        public async Task<bool> EnqueueItemAsync(PlaylistRequestItem item)
        {
            if(item == null)
            {
                return false;
            }

            //Todo: Apply logic to handle when queue is full?
            await _queue.Writer.WriteAsync(item);

            return true;
        }

        public async Task<PlaylistRequestItem?> DequeueItemAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }

    }
}
