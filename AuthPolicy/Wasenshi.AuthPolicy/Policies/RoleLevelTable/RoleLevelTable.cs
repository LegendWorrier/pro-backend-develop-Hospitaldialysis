using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Wasenshi.AuthPolicy.Policies
{
    /// <summary>
    /// Tabel for role level. Any role that hasn't been set or added manually to this table will have the default level of 0.
    /// </summary>
    public class RoleLevelTable : IEnumerable<KeyValuePair<string, int>>
    {
        protected ImmutableDictionary<string, int> Role;

        public RoleLevelTable(ImmutableDictionary<string, int> role)
        {
            Role = role;
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return Role.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Role.GetEnumerator();
        }
    }
}
