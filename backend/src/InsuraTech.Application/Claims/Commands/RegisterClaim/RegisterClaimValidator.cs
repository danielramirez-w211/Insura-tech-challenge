using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace InsuraTech.Application.Claims.Commands.RegisterClaim
{
    public sealed class RegisterClaimValidator : AbstractValidator<RegisterClaimCommand>
    {
        public RegisterClaimValidator()
        {
            RuleFor(x => x.PolicyId)
                .NotEmpty().WithMessage("Policy ID is required.");

            RuleFor(x => x.ClaimedAmount)
                .GreaterThan(0).WithMessage("Claimed amount must be greater than zero.");

            RuleFor(x => x.IncidentDate)
                .NotEmpty().WithMessage("Incident date is required.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Incident date cannot be in the future.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000);

            RuleFor(x => x.ResponsibleUser)
                .NotEmpty().WithMessage("Responsible user is required.");
        }

    }
}
