using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Commands.RejectClaim
{
    public sealed record RejectClaimCommand : IRequest<ClaimResponse>
    {
        public Guid ClaimId { get; init; }
        public string Reason { get; init; } = null!;
        public string ResponsibleUser { get; init; } = null!;
    }
}
