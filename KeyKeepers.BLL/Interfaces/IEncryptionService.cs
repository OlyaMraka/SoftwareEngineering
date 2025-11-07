namespace KeyKeepers.BLL.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText, string? associatedData = null);

    string Decrypt(string cipherTextBase64, string? associatedData = null);
}
