using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace PostIQ.API.Middleware
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
            IConfiguration configuration,
            ILogger<JwtAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            var jwtSection = configuration.GetSection("Jwt");
            var signingKey = jwtSection["SigningKey"]
                ?? throw new InvalidOperationException("Jwt:SigningKey is required.");

            _tokenParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                await _next(context);
                return;
            }

            var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>();
            bool requiresAuth = authorizeData is { Count: > 0 };

            var token = ExtractBearerToken(context.Request);

            if (token is not null)
            {
                var principal = ValidateToken(token);
                if (principal is not null)
                {
                    context.User = principal;

                    if (requiresAuth && !CheckAuthorizePolicies(principal, authorizeData!))
                    {
                        await WriteProblemResponse(context, HttpStatusCode.Forbidden,
                            "You do not have the required role or claim to access this resource.");
                        return;
                    }

                    await _next(context);
                    return;
                }

                _logger.LogWarning("Invalid JWT presented for {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteProblemResponse(context, HttpStatusCode.Unauthorized,
                    "The provided token is invalid or expired.");
                return;
            }

            if (requiresAuth)
            {
                await WriteProblemResponse(context, HttpStatusCode.Unauthorized,
                    "A valid Bearer token is required to access this resource.");
                return;
            }

            await _next(context);
        }

        private static string? ExtractBearerToken(HttpRequest request)
        {
            var header = request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(header))
                return null;

            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return header["Bearer ".Length..].Trim();

            return null;
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, _tokenParams, out _);
                return principal;
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

        private static bool CheckAuthorizePolicies(ClaimsPrincipal principal, IReadOnlyList<IAuthorizeData> authorizeData)
        {
            foreach (var auth in authorizeData)
            {
                if (!string.IsNullOrEmpty(auth.Roles))
                {
                    var requiredRoles = auth.Roles
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    bool hasAnyRole = requiredRoles.Any(role => principal.IsInRole(role));
                    if (!hasAnyRole)
                        return false;
                }
            }

            return true;
        }

        private async Task WriteProblemResponse(HttpContext context, HttpStatusCode statusCode, string detail)
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
                detail
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
        }
    }

    public static class JwtAuthorizationMiddlewareExtensions
    {
        //public static IApplicationBuilder UseJwtAuthorization(this IApplicationBuilder app)
        //{
        //    return app.UseMiddleware<JwtAuthorizationMiddleware>();
        //}
    }
}
