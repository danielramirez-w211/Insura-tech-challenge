using FluentValidation;
using InsuraTech.Domain.Policies;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy
{
    public sealed class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
    {
        public CreatePolicyValidator()
        {
            RuleFor(x => x.IdempotencyKey)
                .NotEmpty().WithMessage("Id Idempotency Key is requeired. ");

            RuleFor(x => x.InsuredDocumentId)
                .NotEmpty().WithMessage("Document ID is requeired. ")
                .MaximumLength(12);

            RuleFor(x => x.InsuredBirthDate)
                .NotEmpty().WithMessage("Birth date is requeired. ")
                .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Birth date must be in past. ");

            RuleFor(x => x.CoverageStartDate)
                .NotEmpty().WithMessage("Coverage start date is required.");

            RuleFor(x => x.CoverageEndDate)
                .NotEmpty().WithMessage("Coverage end date is required.")
                .GreaterThan(x => x.CoverageStartDate)
                .WithMessage("End date must be after start date.");

            RuleFor(x => x.MonthlyPremium)
                .GreaterThan(0).WithMessage("Monthly premium must be greater than zero.");

            RuleFor(x => x.InsuredAmount)
                .GreaterThan(0).WithMessage("Insured amount must be greater than zero.");

            // RN-04: Para pólizas de Salud la fecha de inicio no puede ser anterior a hoy.
            When(x => x.Type == PolicyType.Health, () =>
            {
                RuleFor(x => x.CoverageStartDate)
                    .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                    .WithMessage("La fecha de inicio para pólizas de Salud no puede ser anterior a hoy.");
            });
        }
    }
}
