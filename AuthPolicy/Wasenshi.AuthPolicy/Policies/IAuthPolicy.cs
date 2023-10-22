namespace Wasenshi.AuthPolicy
{
    public interface IAuthPolicy<TId> : IConfigurable<IAuthPolicy<TId>>
    {
        bool Validate(ValidateContext<TId> context);
    }
}
