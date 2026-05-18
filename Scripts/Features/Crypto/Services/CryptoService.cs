namespace GameFoundation.Scripts.Features.Crypto.Services
{
    public class CryptoService : ICryptoService
    {
        public string Decrypt(byte[] cipherBytes) => CryptoUtils.Decrypt(cipherBytes);
        public byte[] Encrypt(string plaintext)   => CryptoUtils.Encrypt(plaintext);
    }
}
