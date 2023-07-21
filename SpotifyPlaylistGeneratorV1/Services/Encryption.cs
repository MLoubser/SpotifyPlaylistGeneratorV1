using System.Security.Cryptography;
using System.Text;
using SpotifyPlaylistGeneratorV1.Interfaces;

namespace SpotifyPlaylistGeneratorV1.Services
{
    public class StringEncryptionService : IStringEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StringEncryptionService> _logger;

        private byte[] IV = {
            27, 64, 25, 184, 90, 205, 209, 192,
           203, 208, 169, 19, 236, 156, 246, 56
        };

        public StringEncryptionService(IConfiguration configuration, ILogger<StringEncryptionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private byte[] Derive128BitKeyFromPassword(string password)
        {
            if(string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("AES Password");
            }

            return Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), Array.Empty<byte>(), 1000, HashAlgorithmName.SHA384, 16);
        }

        public async Task<byte[]> EncryptStringAsync(string cleartext)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Derive128BitKeyFromPassword(_configuration["EncryptionService:AESSecret"] ?? throw new Exception("AES secret not defined!"));
                aes.IV = IV;

                using MemoryStream output = new MemoryStream();

                using CryptoStream cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);

                await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(cleartext));
                await cryptoStream.FlushFinalBlockAsync();

                return output.ToArray();
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to encrypt string");
            }

            return Array.Empty<Byte>(); 
        }

        public async Task<string?> DecryptStringAsync(byte[] encryptedText)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = Derive128BitKeyFromPassword(_configuration["EncryptionService:AESSecret"] ?? throw new Exception("AES secret not defined!"));
                aes.IV = IV;

                using MemoryStream input = new MemoryStream(encryptedText);
                using CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);

                using MemoryStream output = new MemoryStream();

                await cryptoStream.CopyToAsync(output);

                return Encoding.Unicode.GetString(output.ToArray());
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to decrypt string");
            }
            return null;
        }
    }
}
