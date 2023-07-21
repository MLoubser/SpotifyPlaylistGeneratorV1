using Microsoft.AspNetCore.Mvc;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ISpotify _spotify;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(ISpotify spotify, ILogger<CallbackController> logger)
        {
            _spotify = spotify;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Spotify(string state, string? Code = null,  string? error = null)
        {
            //Validate CSRF?
            if(!String.IsNullOrEmpty(state) && !string.IsNullOrEmpty(Code))
            {
                await _spotify.GetAccessTokens(Code);
            }
            else if(!String.IsNullOrEmpty(error))
            {
                _logger.LogWarning($"Login Failed - {error}");
            }


            return RedirectToAction("Index", "Home");
        }
    }
}
