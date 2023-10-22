namespace Wasenshi.AuthPolicy
{
    public interface IConfigurable<TIAuthPolicy> : IConfigurable where TIAuthPolicy : class
    {
    }

    public interface IConfigurable
    {
    }
}
