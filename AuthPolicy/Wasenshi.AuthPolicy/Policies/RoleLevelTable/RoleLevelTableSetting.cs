using System.Collections.Generic;
using System.Collections.Immutable;

namespace Wasenshi.AuthPolicy.Policies
{
    /// <summary>
    /// Setup tabel for role levels. Any role that hasn't been set or added manually to this table will have the default level of 0.
    /// </summary>
    public class RoleLevelTableSetting
    {
        public Dictionary<string, int> Roles { get; } = new Dictionary<string, int>();

        public RoleLevelTableSetting SetRole(string role, int level)
        {
            if (Roles.ContainsKey(role))
            {
                Roles[role] = level;
            }
            else
            {
                Roles.Add(role, level);
            }
            return this;
        }

        public ImmutableDictionary<string, int> GetImmutableTable()
        {
            return Roles.ToImmutableDictionary();
        }
    }
}
