namespace AMYL.Api.Shared.Authorization;

public static class Permissions
{
    public const string View = "view";
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
}

public static class Modules
{
    public const string Memories = "memories";
    public const string Collections = "collections";
    public const string Account = "account";
}

public static class CustomClaimTypes
{
    public const string Permission = "permission";
}
