using AMYL.Api.Shared.Abstractions;

namespace AMYL.Api.Web.Security
{
    public class CookiesExtensions
    {
        private const string AccessTokenCookie = "access_token";
        private const string RefreshTokenCookie = "refresh_token";

        public static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                response.Cookies.Append(
                    AccessTokenCookie,
                    accessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddMinutes(15),
                        SameSite = SameSiteMode.Lax
                    });
                response.Cookies.Append(
                   RefreshTokenCookie,
                   refreshToken,
                   new CookieOptions
                   {
                       HttpOnly = true,
                       Secure = true,
                       Expires = DateTime.UtcNow.AddDays(7),
                       SameSite = SameSiteMode.Lax
                   });

            }
        }
        public static void ClearCookies(HttpResponse httpResponse)
        {
            httpResponse.Cookies.Delete(AccessTokenCookie);
            httpResponse.Cookies.Delete(RefreshTokenCookie);
        }

        public static Result<string> GetRefreshToken(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken))
            {
                return Result<string>.Unauthorized();
            }
            return Result<string>.Success(refreshToken);
        }
    }
}
