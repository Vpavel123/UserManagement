using FluentValidation;
using UserManagement.Model;

namespace UserManagement.Service
{
    public class UserValidator : AbstractValidator<User>
    {
        private readonly ApplicationDdContext _dbContext;

        public UserValidator(ApplicationDdContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .Must(BeUniqueEmail).WithMessage("Email must be unique.");
        }

        private bool BeUniqueEmail(string email)
        {
            return !_dbContext.user.Any(u => u.Email == email);
        }
    }
}
