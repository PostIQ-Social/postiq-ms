using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PostIQ.Core.Middlewares.Jwt
{
    public sealed class JwtAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidationParameters _tokenParams;
        private readonly ILogger<JwtAuthorizationMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public JwtAuthorizationMiddleware(
            RequestDelegate next,
            IOptions<JwtAuthOptions> options,
            ILogger<JwtAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            var opt = options.Value;
            if (string.IsNullOrEmpty(opt.SigningKey))
            {
                throw new InvalidOperationException($"{JwtAuthOptions.SectionName}:{nameof(opt.SigningKey)} is required.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(opt.SigningKey);
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException($"{JwtAuthOptions.SectionName}:{nameof(opt.SigningKey)} must be at least 32 UTF-8 bytes.");
            }

            _tokenParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = !string.IsNullOrEmpty(opt.Issuer),
                ValidIssuer = opt.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(opt.Audience),
                ValidAudience = opt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(opt.ClockSkewSeconds)
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //var endpoint = context.GetEndpoint();
            //var endpoint = context.GetEndpoint();

            //if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            //{
            //    await _next(context);
            //    return;
            //}

            //var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>();
            //bool requiresAuth = authorizeData is { Count: > 0 };

            var token = ExtractBearerToken(context.Request);

            if (token is not null)
            {
                var principal = ValidateToken(token);
                if (principal is not null)
                {
                    context.User = principal;

                    //if (requiresAuth && !CheckRoles(principal, authorizeData!))
                    //{
                    //    await WriteProblemResponse(context, HttpStatusCode.Forbidden,
                    //        "You do not have the required role to access this resource.");
                    //    return;
                    //}

                    await _next(context);
                    return;
                }

                _logger.LogWarning("Invalid JWT for {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteProblemResponse(context, HttpStatusCode.Unauthorized,
                    "The provided token is invalid or expired.");
                return;
            }

            //if (requiresAuth)
            //{
            //    await WriteProblemResponse(context, HttpStatusCode.Unauthorized,
            //        "A valid Bearer token is required to access this resource.");
            //    return;
            //}

            await _next(context);
        }

        private static string? ExtractBearerToken(HttpRequest request)
        {
            var header = request.Headers["Authorization"].ToString();
            if (header.Length > 7 && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return header[7..].Trim();
            }

            return null;
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                return handler.ValidateToken(token, _tokenParams, out _);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogInformation("Rejected expired JWT");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "JWT validation failed");
                return null;
            }
        }

        private static bool CheckRoles(ClaimsPrincipal principal, IReadOnlyList<IAuthorizeData> authorizeData)
        {
            foreach (var auth in authorizeData)
            {
                if (string.IsNullOrEmpty(auth.Roles))
                    continue;

                var required = auth.Roles
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (!required.Any(principal.IsInRole))
                    return false;
            }

            return true;
        }

        private static async Task WriteProblemResponse(HttpContext context, HttpStatusCode statusCode, string detail)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = $"https://httpstatuses.io/{(int)statusCode}",
                title = statusCode switch
                {
                    HttpStatusCode.Unauthorized => "Unauthorized",
                    HttpStatusCode.Forbidden => "Forbidden",
                    _ => "Error"
                },
                status = (int)statusCode,
                detail = detail
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
        }
    }
}
