using System.Security.Cryptography;
using System.Text;

namespace PostIQ.Identity.Services
{
    public class PasswordHasherService
    {
        private const int SaltSize = 16;
        private const int SubKeySize = 32;
        private const int Iterations = 100_000;
        private const byte FormatVersion = 1;

        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] subKey = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, SubKeySize);

            var payload = new byte[1 + SaltSize + SubKeySize];
            payload[0] = FormatVersion;
            Buffer.BlockCopy(salt, 0, payload, 1, SaltSize);
            Buffer.BlockCopy(subKey, 0, payload, 1 + SaltSize, SubKeySize);

            return Convert.ToBase64String(payload);
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var payload = Convert.FromBase64String(storedHash);
                if (payload.Length != 1 + SaltSize + SubKeySize || payload[0] != FormatVersion) return false;

                var salt = new byte[SaltSize];
                Buffer.BlockCopy(payload, 1, salt, 0, SaltSize);
                var expected = new byte[SubKeySize];
                Buffer.BlockCopy(payload, 1 + SaltSize, expected, 0, SubKeySize);
                var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, SubKeySize);

                return CryptographicOperations.FixedTimeEquals(expected, actual);
            }
            catch 
            {

                return false;
            }
            
        }
    }
}
