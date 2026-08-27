namespace AMYL.Api.Shared.Extensions;

/// <summary>
/// Named <c>AuthCookies</c> rather than <c>CookieExtensions</c> because the
/// latter collides with <c>Microsoft.Extensions.DependencyInjection.CookieExtensions</c>.
/// </summary>
public static class AuthCookies
{
    public const string AccessTokenCookie = "access_token";
    public const string RefreshTokenCookie = "refresh_token";

    public static void SetAuthCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        response.Cookies.Append(AccessTokenCookie, accessToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
            Expires = DateTime.UtcNow.AddMinutes(7),
        });

        response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = true,
            Expires = DateTime.UtcNow.AddDays(7),
        });
    }

    public static void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookie);
        response.Cookies.Delete(RefreshTokenCookie);
    }
}
