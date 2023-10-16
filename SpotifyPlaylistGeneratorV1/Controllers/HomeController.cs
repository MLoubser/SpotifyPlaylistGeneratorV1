using Microsoft.AspNetCore.Mvc;
using SpotifyPlaylistGeneratorV1.Models;
using System.Diagnostics;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPlaylistCreateQueue _playlistQueue;

        public HomeController(ILogger<HomeController> logger, IStringEncryptionService stringEncryptionService, ISpotify spotify, IPlaylistCreateQueue playlistQueue, IYoutube youtube)
        {
            _playlistQueue = playlistQueue;
        }

        public async Task<IActionResult> Index()
        {

            var item1 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "rMNLBozi-IA",
                PlaylistName = "First Playlist"
            };

            await _playlistQueue.EnqueueItemAsync(item1);

            var item2 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "IEzrEM3EkBM",
                PlaylistName = "Second Playlist"
            };

            await _playlistQueue.EnqueueItemAsync(item2);


            var item3 = new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "UJm5C5XCOD4",
                PlaylistName = "Third Playlist"
            };

            await _playlistQueue.EnqueueItemAsync(item3);
            

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
