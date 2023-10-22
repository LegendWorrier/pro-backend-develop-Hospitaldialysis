using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<ShareResource> localizer;

        public LocalizationIdentityErrorDescriber(IStringLocalizer<ShareResource> localizer)
        {
            this.localizer = localizer;
        }

        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = localizer["DefaultError"] }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError() { Code = nameof(DuplicateEmail), Description = string.Format(localizer["DuplicateEmail"], email) }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(localizer["DuplicateName"], userName) }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = localizer["ExternalLoginExists"] }; }
        public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(localizer["InvalidEmail"], email) }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = localizer["InvalidToken"] }; }
        public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = string.Format(localizer["InvalidUserName"], userName) }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = localizer["PasswordMismatch"] }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(localizer["PasswordTooShort"], length) }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = localizer["PasswordRequireNonLetterOrDigit"] }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = localizer["PasswordRequireDigit"] }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = localizer["PasswordRequiresLower"] }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = localizer["PasswordRequiresUpper"] }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = localizer["UserAlreadyHasPassword"] }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(localizer["UserAlreadyInRole"], role) }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = string.Format(localizer["UserNotInRole"], role) }; }

    }
}
