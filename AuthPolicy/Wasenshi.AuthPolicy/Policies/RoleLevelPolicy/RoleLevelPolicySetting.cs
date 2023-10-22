namespace Wasenshi.AuthPolicy.Policies
{
    public class RoleLevelPolicySetting<TId> : IConfigurable<RoleLevelPolicy<TId>>
    {
        /// <summary>
        /// Specify whether users have permission for themeselves or not.
        /// </summary>
        public bool AllowSelf { get; set; } = true;

        public bool AllowSameLevel { get; set; } = false;
    }
}
