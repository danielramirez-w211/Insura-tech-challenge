using InsuraTech.Application.LifePlans.DTOs;
using MediatR;

namespace InsuraTech.Application.LifePlans.Queries.GetLifePlans;

public sealed record GetLifePlansQuery : IRequest<IReadOnlyList<LifePlanDto>>;
