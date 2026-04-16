using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetMyProfile;

public sealed class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, UserResponse>
{
    private readonly IUserRepository _users;

    public GetMyProfileHandler(IUserRepository users) => _users = users;

    public async Task<UserResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"Usuario '{request.UserId}' no encontrado.");

        return user.ToResponse();
    }
}
