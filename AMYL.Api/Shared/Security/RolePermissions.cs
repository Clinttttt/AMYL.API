using AMYL.Api.Shared.Domain.Entities;

namespace AMYL.Api.Shared.Security
{
    public static class RolePermissions
    {
        public static IEnumerable<string> GetPermissions(Users user)
        {
            return user.Role switch
            {
                Roles.Owner =>
                [
                $"{Modules.Memories}.{Permissions.View}",
                $"{Modules.Memories}.{Permissions.Create}",
                $"{Modules.Memories}.{Permissions.Update}",
                $"{Modules.Memories}.{Permissions.Delete}",

                $"{Modules.Collections}.{Permissions.View}",
                $"{Modules.Collections}.{Permissions.Create}",
                $"{Modules.Collections}.{Permissions.Update}",
                $"{Modules.Collections}.{Permissions.Delete}"
                ],
                Roles.Viwer =>
                [
                $"{Modules.Collections}.{Permissions.View}",
                $"{Modules.Memories}.{Permissions.View}",
                    ],
                _ => []
            };
        }
    }
}
