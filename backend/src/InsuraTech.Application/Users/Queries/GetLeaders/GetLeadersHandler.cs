using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetLeaders;

public sealed class GetLeadersHandler : IRequestHandler<GetLeadersQuery, IReadOnlyList<UserResponse>>
{
    private readonly IUserRepository _users;

    public GetLeadersHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<UserResponse>> Handle(
        GetLeadersQuery request, CancellationToken cancellationToken)
    {
        var leaders = await _users.GetAllLeadersAsync(cancellationToken);
        return leaders.Select(u => u.ToResponse()).ToList().AsReadOnly();
    }
}
