using System.Linq;

namespace Wasenshi.AuthPolicy.Policies
{
    /// <summary>
    /// This policy uses your defined and pre-defined roles combined together and check with role level table to match the permission.
    /// You can customize role level table in the configuration on startup.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public class RoleLevelPolicy<TId> : RoleLevelPolicySetting<TId>, IAuthPolicy<TId>
    {
        protected RoleLevelTable RoleLevel;

        public RoleLevelPolicy(RoleLevelTable roleLevel)
        {
            RoleLevel = roleLevel;
        }

        public virtual bool Validate(ValidateContext<TId> context)
        {
            if (context.UserId.Equals(context.OwnerId))
            {
                return AllowSelf;
            }

            int max = RoleLevel.Max(x => x.Value);

            int userLevel = context.UserIsGlobal ? max : GetHighestLevel(context.UserRoles);
            int ownerLevel = context.OwnerIsGlobal ? max : GetHighestLevel(context.OwnerRoles);

            return AllowSameLevel ? userLevel >= ownerLevel : userLevel > ownerLevel;
        }

        protected int GetHighestLevel(string[] roles)
        {
            return roles.Join(RoleLevel, s => s, l => l.Key, (role, level) => level.Value).OrderByDescending(x => x).FirstOrDefault();
        }
    }
}
