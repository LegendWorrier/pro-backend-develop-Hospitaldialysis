using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic
{
    public static class UserUtil
    {
        public static string GetName(this IUser user)
        {
            if (user == null)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName : (user.FirstName + " " + user.LastName);
        }
    }
}
