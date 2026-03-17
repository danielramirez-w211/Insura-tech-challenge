using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Application.Claims.DTOs;
using MediatR;

namespace InsuraTech.Application.Claims.Queries.GetClaimById
{
    public sealed record GetClaimByIdQuery : IRequest<ClaimResponse>
    {
        public Guid ClaimId { get; init; }
    }
}
