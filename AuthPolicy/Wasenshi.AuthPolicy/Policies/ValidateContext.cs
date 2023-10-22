namespace Wasenshi.AuthPolicy
{
    public class ValidateContext<TId>
    {
        public ValidateContext(TId userId, TId ownerId, string[] userRoles, string[] ownerRoles, bool userGlobal, bool ownerGlobal)
        {
            UserId = userId;
            OwnerId = ownerId;
            UserRoles = userRoles;
            OwnerRoles = ownerRoles;
            UserIsGlobal = userGlobal;
            OwnerIsGlobal = ownerGlobal;
        }

        public TId UserId { get; }
        public TId OwnerId { get; }
        public string[] UserRoles { get; }
        public string[] OwnerRoles { get; }

        public bool UserIsGlobal { get; }
        public bool OwnerIsGlobal { get; }
    }
}
