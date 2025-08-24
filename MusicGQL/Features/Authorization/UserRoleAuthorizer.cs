using MusicGQL.Features.Users.Roles;

namespace MusicGQL.Features.Authorization;

public class UserRoleAuthorizer
{
    public bool Has(UserRoles roles, UserRoles required) => (roles & required) == required;

    public bool HasAny(UserRoles roles, params UserRoles[] required)
    {
        foreach (var r in required)
        {
            if ((roles & r) == r)
                return true;
        }
        return false;
    }

    public bool IsAdmin(UserRoles roles) => Has(roles, UserRoles.Admin);

    public bool CanCreatePlaylists(UserRoles roles) =>
        HasAny(roles, UserRoles.Admin, UserRoles.CreatePlaylists);

    public bool CanTriggerDownloads(UserRoles roles) =>
        HasAny(roles, UserRoles.Admin, UserRoles.TriggerDownloads);

    public bool CanManageUserRoles(UserRoles roles) =>
        HasAny(roles, UserRoles.Admin, UserRoles.ManageUserRoles);

    public bool CanViewDownloads(UserRoles roles) =>
        HasAny(roles, UserRoles.Admin, UserRoles.ViewDownloads);

    public bool CanEditExternalAuth(UserRoles roles) =>
        HasAny(roles, UserRoles.Admin, UserRoles.EditExternalAuth);
}
