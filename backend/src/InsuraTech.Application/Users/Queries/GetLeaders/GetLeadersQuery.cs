using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetLeaders;

public sealed record GetLeadersQuery : IRequest<IReadOnlyList<UserResponse>>;
