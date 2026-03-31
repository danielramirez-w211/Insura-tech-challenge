using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace InsuraTech.Application.Policies.Commands.RenewPolicy
{
    public sealed class RenewPolicyValidator : AbstractValidator<RenewPolicyCommand>
    {
        public RenewPolicyValidator() {
        
            RuleFor(x => x.PolicyId)
                .NotEmpty()
                .WithMessage("Policy ID is requiered");
            RuleFor(x => x.NewCoverageEndDate)
                    .NotEmpty()
                    .WithMessage("New coverage start date is requiered");
            RuleFor(x => x.NewCoverageEndDate)
            .NotEmpty().WithMessage("New coverage end date is required.")
            .GreaterThan(x => x.NewCoverageStartDate)
            .WithMessage("End date must be after start date.");

        }
    }
}
