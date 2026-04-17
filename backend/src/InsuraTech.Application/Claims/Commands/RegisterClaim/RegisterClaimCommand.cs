using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using InsuraTech.Domain.Claims;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.RegisterClaim
{
    public sealed record RegisterClaimCommand : IRequest<ClaimResponse>
    {
        public Guid    PolicyId            { get; init; }
        public ClaimType Type              { get; init; }
        public decimal ClaimedAmount       { get; init; }
        public DateOnly IncidentDate       { get; init; }
        public string  Description         { get; init; } = null!;
        public string  ResponsibleUser     { get; init; } = null!;

        /// <summary>Se inyecta desde el controller vía JWT. No viene en el body del request.</summary>
        public Guid?   CreatedByAdvisorId  { get; init; }
    }
}
