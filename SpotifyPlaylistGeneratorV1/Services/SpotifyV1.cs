using SpotifyPlaylistGeneratorV1.Interfaces;
using Microsoft.AspNetCore.Identity;
using SpotifyPlaylistGeneratorV1.Authentication;
using System.Web;
using System.Security.Claims;
using SpotifyPlaylistGeneratorV1.Models.Spotify;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistGeneratorV1.Models.Config;

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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient httpClient;
        private readonly IStringEncryptionService _stringEncryptionService;
        private readonly ApplicationDbContext _dbContext;

        private byte[]? encryptedApiKey { get; set; } = null;
        private byte[]? encryptedRefreshToken { get; set; } = null;
        private bool isSignedIn { get; set; } = false;
        private string? userExternalId { get; set; } = null;

        public SpotifyV1(ILogger<SpotifyV1> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor contextAccessor, IStringEncryptionService stringEncryptionService, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = contextAccessor;
            _stringEncryptionService = stringEncryptionService;
            _dbContext = dbContext;

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

            //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "APITOKEN");


            InitializeComponent().Wait();
        }

        private async Task<bool> InitializeComponent()
        {
            isSignedIn = false;

            string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

            var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _dbContext.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == UserName) : null;

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
               
                await _dbContext.SpotifyUser.AddAsync(new ()
                {
                    UserName = UserName,
                });

                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch(Exception e)
                {
                    //TODO: Handle Exception
                    _logger.LogError(e, "InitializeComponent - Failed to save db context");
                }
            }

            if(isSignedIn)
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",await _stringEncryptionService.DecryptStringAsync(encryptedApiKey ?? Array.Empty<byte>()) ?? "");

                currentUser = (!String.IsNullOrEmpty(UserName)) ? await _dbContext.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == UserName) : null;

                if(currentUser != null && currentUser.ExternalId == null)
                {
                    var userDetails = await GetCurrentUserProfileAsync();

                    if (userDetails != null)
                    {
                        currentUser.ExternalId = userDetails.id;

                        _dbContext.Entry(currentUser).State = EntityState.Modified;

                        try
                        {
                            await _dbContext.SaveChangesAsync();
                            userExternalId = currentUser.ExternalId;
                        }
                        catch (Exception e)
                        {
                            //TODO: Handle Exception
                            _logger.LogError(e, "InitializeComponent - Failed to save db context");
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

                //Check if user details is populated from /me

            }
               
            return true;
        }


        private async Task<bool> ClearTokens()
        {
            string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

            var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _dbContext.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == UserName) : null;

            if (currentUser != null)
            {
                currentUser.ApiToken = null;
                currentUser.RefreshToken = null;
                currentUser.TokenExpireDate = null;


                _dbContext.Entry(currentUser).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();

                    encryptedApiKey = null;
                    encryptedRefreshToken = null;
                    isSignedIn = false;
                }
                catch (Exception e)
                {
                    //TODO: Handle Exception
                    _logger.LogError(e, "ClearTokens - Failed to save db context");
                }
            }
            
            return true;
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
                        string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

                        var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _dbContext.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == UserName) : null;

                        byte[] encryptedBytes = await _stringEncryptionService.EncryptStringAsync(Data.access_token);

                        if (currentUser != null)
                        {
                            currentUser.ApiToken = Convert.ToBase64String(encryptedBytes);
                            currentUser.TokenExpireDate = DateTime.Now.AddSeconds(Data.expires_in);

                            _dbContext.Entry(currentUser).State = EntityState.Modified;

                            try
                            {
                                await _dbContext.SaveChangesAsync();
                            }
                            catch(Exception e)
                            {
                                //TODO: Handle Exception
                                _logger.LogError(e, "GetNewTokenFromRefreshToken - Failed to save db context");

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
                    string UserName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "";

                    var currentUser = (!String.IsNullOrEmpty(UserName)) ? await _dbContext.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == UserName) : null;

                    byte[] encAccessToken = await _stringEncryptionService.EncryptStringAsync(Data.access_token);
                    byte[] encRefreshToken = await _stringEncryptionService.EncryptStringAsync(Data.refresh_token);

                    if (currentUser != null)
                    {
                        currentUser.ApiToken = Convert.ToBase64String(encAccessToken);
                        currentUser.RefreshToken = Convert.ToBase64String(encRefreshToken);
                        currentUser.TokenExpireDate = DateTime.Now.AddSeconds(Data.expires_in);

                        _dbContext.Entry(currentUser).State = EntityState.Modified;

                        try
                        {
                            await _dbContext.SaveChangesAsync();
                        }
                        catch(Exception e)
                        {
                            //TODO: Handle Exception
                            _logger.LogError(e, "GetAccessTokens - Failed to save db context");

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

        public async Task<bool> CreateNewPlaylist(string PlaylistName, string? PlaylistDescription = null, bool PublicPlaylist = false)
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
                    var data = response.Content.ReadFromJsonAsync<CreatePlaylistResponseModel>();

                    if(data != null)
                    {
                        return true;
                    }
                }
               
                _logger.LogWarning("Failed to create new playlist - SpotifyService");
                
            }
            return false;
        }

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
