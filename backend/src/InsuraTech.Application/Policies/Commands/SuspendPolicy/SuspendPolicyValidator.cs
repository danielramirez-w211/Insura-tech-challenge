using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace InsuraTech.Application.Policies.Commands.SuspendPolicy
{
    public sealed class SuspendPolicyValidator : AbstractValidator<SuspendPolicyCommand>
    {
        public SuspendPolicyValidator()
        {
            RuleFor(x=> x.PolicyId)
                .NotEmpty()
                .WithMessage("Policy Id is requiered. ");
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Suspension reason is requiered. ")
                .MaximumLength(500);

        }

    }
}
