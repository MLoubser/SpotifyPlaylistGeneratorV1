using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class CreatePlaylistService : BackgroundService
    {
        private readonly ILogger<CreatePlaylistService> _logger;
        public IPlaylistCreateQueue TaskQueue { get; }
        public IServiceProvider ServiceProvider { get; }

        public CreatePlaylistService(ILogger<CreatePlaylistService> logger, IPlaylistCreateQueue queue, IServiceProvider serviceProvider) 
        { 
            _logger = logger;
            TaskQueue = queue;
            ServiceProvider = serviceProvider;
        }
        
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting background service");
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stopToken)
        {
            while(!stopToken.IsCancellationRequested)
            {
                var newTask = await TaskQueue.DequeueItemAsync(stopToken);

                if(newTask != null)
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var spotifyInstance = scope.ServiceProvider.GetRequiredService<ISpotify>();
                        var youtubeInstance = scope.ServiceProvider.GetRequiredService<IYoutube>();

                        if (spotifyInstance != null && youtubeInstance != null)
                        {
                            var listOfDescriptionItems = youtubeInstance.ProcessDescriptionForTrackList(await youtubeInstance.FetchVideoDescriptionByIdAsync(newTask.YouTubeUrl));
                            
                            if(listOfDescriptionItems.Count > 0)
                            {
                                await spotifyInstance.InitializeComponentAsync(newTask.UserName);
                                var playlistId = await spotifyInstance.CreateNewPlaylistAsync(newTask.PlaylistName);

                                if (playlistId != null)
                                {
                                    var listOfTrackIds = new List<string>();

                                    foreach (var item in listOfDescriptionItems)
                                    {
                                        var id = await spotifyInstance.GetSpotifyInternalIdFromDescriptionAsync(item);

                                        if (id != null)
                                        {
                                            listOfTrackIds.Add(id);
                                        }
                                    }

                                    if (listOfTrackIds.Count > 0)
                                    {
                                        var addResult = await spotifyInstance.AddTracksToPlaylistAsync(playlistId, listOfTrackIds);

                                    }
                                    else
                                    {
                                        _logger.LogWarning("Background service - Failed to find any spotify Items for request");
                                    }
                                }

                            }
                            
                            
                        }
                    }
                }

                await Task.Delay(100,stopToken);
            }
        }

        public override async Task StopAsync(CancellationToken token)
        {
            _logger.LogInformation("Stopping background service");

            await base.StopAsync(token);
        }

    }
}
