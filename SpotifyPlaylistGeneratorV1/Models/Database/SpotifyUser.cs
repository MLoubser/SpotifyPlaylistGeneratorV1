namespace SpotifyPlaylistGeneratorV1.Models.Database
{
    public class SpotifyUserModel
    {
        public int Id { get; set; }
        public string? ExternalId { get; set; }
        public string UserName { get; set; } = default!;
        public DateTime? TokenExpireDate { get; set; }
        public string? RefreshToken { get; set; }
        public string? ApiToken { get; set; }
    }
}
