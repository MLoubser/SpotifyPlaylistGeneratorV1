using Microsoft.AspNetCore.Mvc;
using SpotifyPlaylistGeneratorV1.Models;
using System.Diagnostics;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringEncryptionService _stringEncryption;
        private readonly ISpotify _spotify;
        private readonly IYoutube _youtube;
        private readonly IPlaylistCreateQueue _playlistQueue;

        public HomeController(ILogger<HomeController> logger, IStringEncryptionService stringEncryptionService, ISpotify spotify, IPlaylistCreateQueue playlistQueue, IYoutube youtube)
        {
            _logger = logger;
            _stringEncryption = stringEncryptionService;
            _spotify = spotify;
            _playlistQueue = playlistQueue;
            _youtube = youtube;
        }

        public async Task<IActionResult> Index()
        {

            var item1 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "rMNLBozi-IA",
                PlaylistName = "First Playlist"
            };

            //await _playlistQueue.EnqueueItemAsync(item1);

            var item2 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "IEzrEM3EkBM",
                PlaylistName = "Second Playlist"
            };

            //var activeItem = item1;

            //await _playlistQueue.EnqueueItemAsync(item2);


            var item3 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "UJm5C5XCOD4",
                PlaylistName = "Third Playlist"
            };

            await _playlistQueue.EnqueueItemAsync(item3);
            /**************************************************************************************************************/
            //var listOfDescriptionItems = _youtube.ProcessDescriptionForTrackList(await _youtube.FetchVideoDescriptionByIdAsync(activeItem.YouTubeUrl));

            //if (listOfDescriptionItems.Count > 0)
            //{
            //    await _spotify.InitializeComponentAsync(activeItem.UserName);
            //    var playlistId = await _spotify.CreateNewPlaylistAsync(activeItem.PlaylistName);

            //    if (playlistId != null)
            //    {
            //        var listOfTrackIds = new List<string>();

            //        foreach (var item in listOfDescriptionItems)
            //        {
            //            var id = await _spotify.GetSpotifyInternalIdFromDescriptionAsync(item);

            //            if (id != null && !listOfTrackIds.Contains(id))
            //            {
            //                listOfTrackIds.Add(id);
            //            }
            //        }

            //        if (listOfTrackIds.Count > 0)
            //        {
            //            var addResult = await _spotify.AddTracksToPlaylistAsync(playlistId, listOfTrackIds);

            //        }
            //        else
            //        {
            //            _logger.LogWarning("Background service - Failed to find any spotify Items for request");
            //        }

            //        var test = "";
            //    }

            //}
            /**************************************************************************************************************/

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
