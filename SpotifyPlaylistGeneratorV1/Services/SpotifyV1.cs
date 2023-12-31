﻿using SpotifyPlaylistGeneratorV1.Interfaces;
using SpotifyPlaylistGeneratorV1.Authentication;
using SpotifyPlaylistGeneratorV1.Models.Spotify;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistGeneratorV1.Models.Config;
using System.Runtime.CompilerServices;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class SpotifyV1 : ISpotify
    {

        private static readonly string REQUEST_ACCESS_TOKEN_URL = "https://accounts.spotify.com/api/token";
        
        //LoadConstants
        private readonly string BASE_URL;
        private readonly double REFRESH_TOKEN_EXPIRE_MINUTES;
        private readonly string CLIENT_ID;
        private readonly string ClIENT_SECRET;
        private readonly string APP_NAME;
        private readonly string REDIRECT_URL;
        private readonly string REQUIRED_SCOPE;
        
        private readonly ILogger<SpotifyV1> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient httpClient;
        private readonly IStringEncryptionService _stringEncryptionService;
        private readonly ISpotifyUserRepository _spotifyUserRepository;

        private byte[]? encryptedApiKey { get; set; } = null;
        private byte[]? encryptedRefreshToken { get; set; } = null;
        private bool isSignedIn { get; set; } = false;
        private string? userExternalId { get; set; } = null;
        private string UserName { get; set; } = string.Empty;

        public SpotifyV1(ILogger<SpotifyV1> logger, IHttpContextAccessor contextAccessor, IStringEncryptionService stringEncryptionService, ISpotifyUserRepository spotifyUserRepository, IConfiguration configuration)
        {
            _logger = logger;
            _httpContextAccessor = contextAccessor;
            _stringEncryptionService = stringEncryptionService;
            _spotifyUserRepository = spotifyUserRepository;

            var config = configuration.GetSection("SpotifyConfig").Get<SpotifyConfigModel>();

            if (config != null)
            {
                BASE_URL = config.BaseUrl;
                REFRESH_TOKEN_EXPIRE_MINUTES = config.RefreshTokenExpireMinutes;
                CLIENT_ID = config.ClientId;
                ClIENT_SECRET = config.ClientSecret;
                APP_NAME = config.AppName;
                REDIRECT_URL = config.RedirectUrl;
                REQUIRED_SCOPE = config.RequiredScope;
            }
            else
            {
                throw new Exception("Failed to configure spotify service");
            }

            //Initialize HttpClient
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BASE_URL);

            InitializeComponentAsync().Wait();
        }

        public async Task<bool> InitializeComponentAsync(string? userNameWithoutContext = null)
        {
            isSignedIn = false;

            UserName = userNameWithoutContext ?? _httpContextAccessor.HttpContext?.User.Identity?.Name ?? String.Empty;

            var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _spotifyUserRepository.FetchUserFromUsernameAsync(UserName) : null;

            if(currentUser != null)
            {
                encryptedApiKey = !String.IsNullOrEmpty(currentUser.ApiToken) ? Convert.FromBase64String(currentUser.ApiToken) : null;
                encryptedRefreshToken = !String.IsNullOrEmpty(currentUser.RefreshToken) ? Convert.FromBase64String(currentUser.RefreshToken) : null;

                double TimeDiff = double.NegativeInfinity;

                if (currentUser.TokenExpireDate.HasValue)
                {
                    TimeDiff = (currentUser.TokenExpireDate?.Subtract(DateTime.Now))?.TotalMinutes ?? double.NegativeInfinity;
                }

                if (encryptedApiKey != null && encryptedRefreshToken != null && TimeDiff > REFRESH_TOKEN_EXPIRE_MINUTES)
                {
                    isSignedIn = true;
                } 
                else if(encryptedRefreshToken != null)
                {
                    //Get new token
                    var success = await GetNewTokenFromRefreshToken();

                    if(success)
                    {
                        isSignedIn = true;
                    }
                    else
                    {
                        await ClearTokens();
                    }
                }
                else
                {
                    await ClearTokens();
                }
            }
            //Create entry for current User
            else if(!string.IsNullOrEmpty(UserName))
            {
                await _spotifyUserRepository.AddUserAsync(new() { UserName = UserName });

            }

            if(isSignedIn)
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",await _stringEncryptionService.DecryptStringAsync(encryptedApiKey ?? Array.Empty<byte>()) ?? "");

                currentUser = (!String.IsNullOrEmpty(UserName)) ? await _spotifyUserRepository.FetchUserFromUsernameAsync(UserName) : null;

                if(currentUser != null && currentUser.ExternalId == null)
                {
                    var userDetails = await GetCurrentUserProfileAsync();

                    if (userDetails != null)
                    {
                        currentUser.ExternalId = userDetails.id;

                        var result = await _spotifyUserRepository.UpdateUserAsync(currentUser);

                        if(result)
                        {
                            userExternalId = currentUser.ExternalId;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("InitializeComponent - Failed to get user profile!");
                    }
                }
                else if(currentUser != null)
                {
                    userExternalId = currentUser.ExternalId;
                }
            }
               
            return true;
        }


        private async Task<bool> ClearTokens()
        {
            return await _spotifyUserRepository.ClearUserTokensAsync(UserName);
        }


        private async Task<bool> GetNewTokenFromRefreshToken()
        {
            if(encryptedRefreshToken != null)
            {
                HttpClient AuthClient = new HttpClient();
                AuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{ClIENT_SECRET}")));

                var Response =  await AuthClient.PostAsync(new Uri(REQUEST_ACCESS_TOKEN_URL), new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type","refresh_token"),
                    new KeyValuePair<string, string>("refresh_token",await _stringEncryptionService.DecryptStringAsync(encryptedRefreshToken) ?? "")
                }));

                if(Response.IsSuccessStatusCode)
                {
                    var Data = await Response.Content.ReadFromJsonAsync<RefreshTokenResponseModel>();

                    if(Data != null)
                    {
                        //string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

                        var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _spotifyUserRepository.FetchUserFromUsernameAsync(UserName) : null;

                        byte[] encryptedBytes = await _stringEncryptionService.EncryptStringAsync(Data.access_token);

                        if (currentUser != null)
                        {
                            currentUser.ApiToken = Convert.ToBase64String(encryptedBytes);
                            currentUser.TokenExpireDate = DateTime.Now.AddSeconds(Data.expires_in);

                            var Result = await _spotifyUserRepository.UpdateUserAsync(currentUser);

                            if(!Result)
                            {
                                return false;
                            }
                         
                        }

                        encryptedApiKey = encryptedBytes;

                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse response from token refresh - SpotifyService");
                    }        
                }
                else
                {
                    _logger.LogWarning("Failed to refresh token - SpotifyService");
                }
            }

            return false;
        }

        public async Task<bool> GetAccessTokens(string accessCode)
        {      
            HttpClient AuthClient = new HttpClient();
            AuthClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{ClIENT_SECRET}")));

            var Response = await AuthClient.PostAsync(new Uri(REQUEST_ACCESS_TOKEN_URL), new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type","authorization_code"),
                new KeyValuePair<string, string>("code", accessCode),
                new KeyValuePair<string, string>("redirect_uri", REDIRECT_URL)
            }));

            if (Response.IsSuccessStatusCode)
            {
                var Data = await Response.Content.ReadFromJsonAsync<AccessTokenResponseModel>();

                if (Data != null)
                {
                    //string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

                    var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _spotifyUserRepository.FetchUserFromUsernameAsync(UserName) : null;

                    byte[] encAccessToken = await _stringEncryptionService.EncryptStringAsync(Data.access_token);
                    byte[] encRefreshToken = await _stringEncryptionService.EncryptStringAsync(Data.refresh_token);

                    if (currentUser != null)
                    {
                        currentUser.ApiToken = Convert.ToBase64String(encAccessToken);
                        currentUser.RefreshToken = Convert.ToBase64String(encRefreshToken);
                        currentUser.TokenExpireDate = DateTime.Now.AddSeconds(Data.expires_in);

                        var Result = await _spotifyUserRepository.UpdateUserAsync(currentUser);

                        if(!Result)
                        {
                            return false;
                        }

                    }

                    encryptedApiKey = encAccessToken;
                    encryptedRefreshToken = encRefreshToken;

                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to parse response from token request - SpotifyService");
                }
            }
            else
            {
                _logger.LogWarning("Failed to request token - SpotifyService");
            }
            

            return false;
        }

        private async Task<CurrentUserProfileResponseModel?>GetCurrentUserProfileAsync()
        {
            if(isSignedIn)
            {
                var response = await httpClient.GetAsync("me");

                if(response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CurrentUserProfileResponseModel>();
                }
                else
                {
                    _logger.LogWarning("Failed to get current user profile - SpotifyService");
                }
            }
            return null;
        }

        public async Task<string?> CreateNewPlaylistAsync(string PlaylistName, string? PlaylistDescription = null, bool PublicPlaylist = false)
        {
            if(isSignedIn)
            {
               
                if(String.IsNullOrEmpty(PlaylistDescription))
                {
                    PlaylistDescription = $"Created by {APP_NAME}";
                }

                var response = await httpClient.PostAsJsonAsync<CreatePlaylistRequestModel>($"users/{userExternalId}/playlists", new CreatePlaylistRequestModel
                {
                    description = PlaylistDescription,
                    name = PlaylistName,
                    @public = PublicPlaylist
                });


                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<CreatePlaylistResponseModel>();

                    if(data != null)
                    {
                        return data.id;
                    }
                }
               
                _logger.LogWarning("Failed to create new playlist - SpotifyService");
                
            }
            return null;
        }

        public async Task<string?> GetSpotifyInternalIdFromDescriptionAsync(string trackDescription)
        {
            if(isSignedIn)
            {
                var requestUrl = $"search?q={/*"track:" +*/ trackDescription}&type=track&limit=1";

                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                     var responseObj = await response.Content.ReadFromJsonAsync<SearchRequestResponseModel>();

                    if(responseObj != null && responseObj.tracks.items.Length > 0)
                    {
                        _logger.LogInformation($"SpotifyService - Found id for search: {trackDescription}    \nArtist: {responseObj.tracks.items.FirstOrDefault()?.artists.FirstOrDefault()?.name}  \nTitle: {responseObj.tracks.items.FirstOrDefault()?.name}");
                        
                        return responseObj.tracks.items.FirstOrDefault()?.id;
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get Internal Id from description - SpotifyService");
                }
            }

            return null;
        }

        public async Task<bool> AddTracksToPlaylistAsync(string playlistId, List<string> ItemIds)
        {
            if (isSignedIn && ItemIds.Count > 0)
            {
                var request = new AddToPlaylistRequestModel()
                {
                    uris = new List<string>(),
                    position = 0
                };

                foreach (var item in ItemIds)
                {
                    request.uris.Add(FormatTrackItemId(item));
                }


                var response = await httpClient.PostAsJsonAsync($"playlists/{playlistId}/tracks", request);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<AddToPlaylistResponseModel>();

                    if (data != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string FormatTrackItemId(string itemId) => $"spotify:track:{itemId}";

        public bool IsLoggedIn()
        {
            return isSignedIn;
        }

        public string GenerateSignupLink(string? CSRFToken = null)
        {
            if(CSRFToken == null)
            {
                CSRFToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }
            
            return "https://accounts.spotify.com/en/authorize" +
                   "?client_id=" + CLIENT_ID +
                   "&response_type=" + "code" +
                   "&redirect_uri=" + REDIRECT_URL +
                   "&state=" + CSRFToken +
                   "&scope=" + REQUIRED_SCOPE;
        }
    }
}
