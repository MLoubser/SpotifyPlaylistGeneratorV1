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
        private readonly IPlaylistCreateQueue _playlistQueue;

        public HomeController(ILogger<HomeController> logger, IStringEncryptionService stringEncryptionService, ISpotify spotify, IPlaylistCreateQueue playlistQueue)
        {
            _logger = logger;
            _stringEncryption = stringEncryptionService;
            _spotify = spotify;
            _playlistQueue = playlistQueue;
        }

        public async Task<IActionResult> Index()
        {

            await _playlistQueue.EnqueueItemAsync(new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "Test 3"
            });

            await _playlistQueue.EnqueueItemAsync(new Models.BackgroundService.PlaylistRequestItem
            {
                UserName = HttpContext?.User.Identity?.Name ?? "",
                YouTubeUrl = "Test 4"
            });


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
