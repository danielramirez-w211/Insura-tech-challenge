using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.StartInvestigation
{
    public sealed record StartInvestigationCommand : IRequest<ClaimResponse>
    {
        public Guid ClaimId { get; init; }
        public string ResponsibleUser { get; init; } = null!;
        public string? Observations { get; init; }
    }
}
