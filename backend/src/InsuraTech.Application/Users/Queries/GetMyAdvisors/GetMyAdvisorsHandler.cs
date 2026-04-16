using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Queries.GetMyAdvisors;

public sealed class GetMyAdvisorsHandler
    : IRequestHandler<GetMyAdvisorsQuery, IReadOnlyList<AdvisorSummaryResponse>>
{
    private readonly IUserRepository   _users;
    private readonly IPolicyRepository _policies;

    public GetMyAdvisorsHandler(IUserRepository users, IPolicyRepository policies)
    {
        _users    = users;
        _policies = policies;
    }

    public async Task<IReadOnlyList<AdvisorSummaryResponse>> Handle(
        GetMyAdvisorsQuery request, CancellationToken cancellationToken)
    {
        var advisors = await _users.GetByLeaderIdAsync(request.LeaderId, cancellationToken);

        // Calcular salesCount en paralelo para todos los asesores
        var tasks = advisors.Select(async advisor =>
        {
            var salesCount = await _policies.CountByAdvisorIdAsync(advisor.Id, cancellationToken);
            return new AdvisorSummaryResponse
            {
                Id          = advisor.Id,
                Email       = advisor.Email,
                AdvisorCode = advisor.AdvisorCode ?? string.Empty,
                IsActive    = advisor.IsActive,
                FirstName   = advisor.Profile.FirstName,
                LastName    = advisor.Profile.LastName,
                SalesCount  = salesCount,
            };
        });

        var results = await Task.WhenAll(tasks);
        return results.ToList().AsReadOnly();
    }
}
