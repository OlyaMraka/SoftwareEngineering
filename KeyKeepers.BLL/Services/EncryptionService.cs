using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using KeyKeepers.BLL.Interfaces;

namespace KeyKeepers.BLL.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] key;

    public EncryptionService(IConfiguration configuration)
    {
        var base64Key = configuration["Encryption:KeyBase64"];

        if (string.IsNullOrWhiteSpace(base64Key))
        {
            throw new InvalidOperationException("Encryption key not found in configuration (Encryption:KeyBase64).");
        }

        key = Convert.FromBase64String(base64Key);

        if (key.Length != 32)
        {
            throw new InvalidOperationException("Encryption key must be 32 bytes (base64-encoded).");
        }
    }

    public string Encrypt(string plainText, string? associatedData = null)
    {
        if (plainText is null)
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);

        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[16];
        var ciphertext = new byte[plaintextBytes.Length];

        using var aesGcm = new AesGcm(key, 16);
        if (!string.IsNullOrEmpty(associatedData))
        {
            var ad = Encoding.UTF8.GetBytes(associatedData);
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag, ad);
        }
        else
        {
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherTextBase64, string? associatedData = null)
    {
        if (cipherTextBase64 is null)
        {
            throw new ArgumentNullException(nameof(cipherTextBase64));
        }

        var allBytes = Convert.FromBase64String(cipherTextBase64);

        var nonce = new byte[12];
        var tag = new byte[16];
        var ciphertext = new byte[allBytes.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(allBytes, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(allBytes, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(allBytes, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

        var plaintextBytes = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, 16);
        if (!string.IsNullOrEmpty(associatedData))
        {
            var ad = Encoding.UTF8.GetBytes(associatedData);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes, ad);
        }
        else
        {
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
        }

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
