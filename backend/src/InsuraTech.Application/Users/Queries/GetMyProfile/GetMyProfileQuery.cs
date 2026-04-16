using InsuraTech.Application.Users.DTOs;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetMyProfile;

public sealed record GetMyProfileQuery(Guid UserId) : IRequest<UserResponse>;
