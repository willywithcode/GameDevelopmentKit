namespace GameFoundation.Scripts.Features.Crypto.Services
{
    public interface ICryptoService
    {
        string Decrypt(byte[] cipherBytes);
        byte[] Encrypt(string plaintext);
    }
}
