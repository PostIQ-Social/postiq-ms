using System.Text;
using System;

namespace PostIQ.Core.Shared.Encrypt
{
    public static class RandomGenerator
    {
        public static string RandomOTP(int iOTPLength)
        {
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sOTP = String.Empty;

            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }

        public static string RandomString(int length, bool isAllowedSpecialChar = false)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            builder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
            if (isAllowedSpecialChar)
                builder.Append("~!@#$%^&*()_+{}<>?[]");
            return new string(Enumerable.Repeat(builder, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Generate a random number between two numbers  
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

    }
}
