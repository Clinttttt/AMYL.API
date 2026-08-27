namespace AMYL.Api.Web.Extensions
{
    public class CookiesExtensions
    {
        public const string AccesssToken = "access_token";
        public const string RefreshToken = "refresh_token";

            public static void SetTCookies(HttpResponse http, string accessToken, string refreshToken)
        {
            if(!string.IsNullOrWhiteSpace(accessToken) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                http.Cookies.Append(AccesssToken, accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(7),
                });
                http.Cookies.Append(RefreshToken, refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                });
            }
        }
        public static void ClearCookies(HttpResponse http)
        {
            http.Cookies.Delete(AccesssToken);
            http.Cookies.Delete(RefreshToken);
        }
    }
}
