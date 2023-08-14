using SpotifyPlaylistGeneratorV1.Authentication;
using SpotifyPlaylistGeneratorV1.Models.Database;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Repositories
{
    public class SpotifyUserRepository : ISpotifyUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpotifyUserRepository> _logger;

        public SpotifyUserRepository(ApplicationDbContext context, ILogger<SpotifyUserRepository> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<SpotifyUserModel?> FetchUserFromUsernameAsync(string username)
        {
            return await _context.SpotifyUser.FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<bool> AddUserAsync(SpotifyUserModel user)
        { 
            await _context.SpotifyUser.AddAsync(user);

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SpotifyUserRepository - Failed to create new user");
            }
            return false;
        }

        public async Task<bool> UpdateUserAsync(SpotifyUserModel user)
        {
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "SpotifyUserRepository - Failed to update user");
            }

            return false;
        }

        public async Task<bool> ClearUserTokensAsync(string username)
        {
            var User = await FetchUserFromUsernameAsync(username);

            if(User != null)
            {
                User.ApiToken = null;
                User.RefreshToken = null;
                User.TokenExpireDate = null;

                _context.Entry(User).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();

                    return true;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "SpotifyUserRepository - Failed to clear user tokens");
                }
            }

            return false;
        }

    }
}
