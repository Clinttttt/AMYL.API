namespace AMYL.Api.Shared.Security
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Name { get; }
    }
}
