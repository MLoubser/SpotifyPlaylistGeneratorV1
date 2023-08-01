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
                        var SpotifyInstance = scope.ServiceProvider.GetRequiredService<ISpotify>();

                        if(SpotifyInstance != null)
                        {
                            await SpotifyInstance.InitializeComponentAsync(newTask.UserName);
                            await SpotifyInstance.CreateNewPlaylist(newTask.YouTubeUrl);
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
