using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetMyAdvisors;

public sealed record GetMyAdvisorsQuery(Guid LeaderId) : IRequest<IReadOnlyList<AdvisorSummaryResponse>>;
