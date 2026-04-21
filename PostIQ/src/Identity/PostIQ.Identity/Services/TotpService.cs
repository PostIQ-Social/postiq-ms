using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace PostIQ.Identity.Services
{
    /// <summary>RFC 6238 TOTP using HMAC-SHA1 (Google Authenticator compatible).</summary>
    public sealed class TotpService
    {
        private const int TimeStepSeconds = 30;
        private const int CodeDigits = 6;

        public string GenerateBase32Secret(int byteLength = 20)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Base32Encode(bytes);
        }

        public bool ValidateCode(string base32Secret, string code, int allowedDriftSteps = 1)
        {
            if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code))
                return false;

            if (!int.TryParse(code.Trim(), out var entered) || code.Trim().Length != CodeDigits)
                return false;

            byte[] secretBytes;
            try
            {
                secretBytes = Base32Decode(base32Secret);
            }
            catch
            {
                return false;
            }

            var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var counter = unix / TimeStepSeconds;

            for (var i = -allowedDriftSteps; i <= allowedDriftSteps; i++)
            {
                var c = (ulong)(counter + i);
                if (ComputeTotp(secretBytes, c) == entered)
                    return true;
            }

            return false;
        }

        private static int ComputeTotp(byte[] secret, ulong counter)
        {
            Span<byte> counterBytes = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64BigEndian(counterBytes, counter);

            var hash = HMACSHA1.HashData(secret, counterBytes);
            var offset = hash[^1] & 0x0f;
            var binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            var otp = binary % (int)Math.Pow(10, CodeDigits);
            return otp;
        }

        private static string Base32Encode(ReadOnlySpan<byte> data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder((data.Length * 8 + 4) / 5);
            int buffer = 0, bitsLeft = 0;
            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    bitsLeft -= 5;
                    output.Append(alphabet[(buffer >> bitsLeft) & 31]);
                }
            }
            if (bitsLeft > 0)
            {
                output.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);
            }
            return output.ToString();
        }

        private static byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.Trim().ToUpperInvariant().Replace(" ", "");
            var output = new List<byte>();
            int buffer = 0, bitsLeft = 0;
            foreach (var c in input)
            {
                var idx = alphabet.IndexOf(c);
                if (idx < 0)
                    throw new FormatException("Invalid base32");

                buffer = (buffer << 5) | idx;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    bitsLeft -= 8;
                    output.Add((byte)(buffer >> bitsLeft));
                }
            }
            return output.ToArray();
        }
    }
}
