namespace AMYL.Api.Features.Authentication.Login
{
    public sealed record TokenResponseDto(string AccessToken, string RefreshToken);
}
