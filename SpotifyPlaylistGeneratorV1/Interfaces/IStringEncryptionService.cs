namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IStringEncryptionService
    {
        Task<byte[]> EncryptStringAsync(string cleartext);
        Task<string?> DecryptStringAsync(byte[] encryptedText);

    }
}
