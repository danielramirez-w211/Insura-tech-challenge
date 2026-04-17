using InsuraTech.Application.Policies.DTOs;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetMyClients;

public sealed record GetMyClientsQuery : IRequest<IEnumerable<ClientSummaryResponse>>
{
    public Guid AdvisorId { get; init; }
}