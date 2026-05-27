using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.Middlewares.Jwt
{
    public sealed class JwtAuthOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Symmetric key for HMAC-SHA256 signing. Must be at least 32 UTF-8 bytes.
        /// </summary>
        public string SigningKey { get; set; } = string.Empty;

        /// <summary>
        /// Allowed clock skew for token lifetime validation. Defaults to 1 minute.
        /// </summary>
        public int ClockSkewSeconds { get; set; } = 60;
    }
}
