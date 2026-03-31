using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Policies;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy
{
    public sealed record CreatePolicyCommand : IRequest<PolicyResponse>
    {
        public string IdempotencyKey { get; init; } = null!;
        public PolicyType Type { get; init; }
        public string InsuredFullName { get; init; } = null!;
        public string InsuredDocumentId { get; init; } = null!;
        public DateOnly InsuredBirthDate { get; init; }
        public DateOnly CoverageStartDate { get; init; }
        public DateOnly CoverageEndDate { get; init; }
        public decimal MonthlyPremium { get; init; }
        public decimal InsuredAmount { get; init; }
    }
}
