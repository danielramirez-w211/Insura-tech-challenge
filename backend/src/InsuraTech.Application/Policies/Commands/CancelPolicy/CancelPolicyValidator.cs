using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace InsuraTech.Application.Policies.Commands.CancelPolicy
{
    public sealed class CancelPolicyValidator : AbstractValidator<CancelPolicyCommand>
    {
        public CancelPolicyValidator()
        {
            RuleFor(x => x.PolicyId)
                .NotEmpty().WithMessage("Policy ID is required.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Cancellation reason is required.")
                .MaximumLength(500);

            RuleFor(x => x.EffectiveDate)
                .NotEmpty().WithMessage("Effective date is required.")
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Effective date cannot be in the past.");
        }
    }
}
