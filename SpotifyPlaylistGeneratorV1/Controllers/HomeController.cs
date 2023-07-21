﻿using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, IStringEncryptionService stringEncryptionService, ISpotify spotify)
        {
            _logger = logger;
            _stringEncryption = stringEncryptionService;
            _spotify = spotify;
        }

        public async Task<IActionResult> Index()
        {
            var TestString = "First encrypt test";

            var encrypted = Convert.ToBase64String(await _stringEncryption.EncryptStringAsync(TestString));

            var decryptedString = await _stringEncryption.DecryptStringAsync(Convert.FromBase64String(encrypted));

            var createdPlaylist = _spotify.CreateNewPlaylist("Testing Playlist");
                 
            return View(new Testing.EncryptTest
            {
                FirstString = TestString,
                EncryptedString = encrypted,
                OutputString = decryptedString
            });
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




namespace Testing
{
    public class EncryptTest
    {
        public string FirstString { get; set; }
        public string EncryptedString { get; set; }
        public string OutputString { get; set; }
    }
}