namespace SpotifyPlaylistGeneratorV1.Interfaces
{
    public interface IStringEncryptionService
    {
        public Task<byte[]> EncryptStringAsync(string cleartext);
        public Task<string?> DecryptStringAsync(byte[] encryptedText);

    }
}
