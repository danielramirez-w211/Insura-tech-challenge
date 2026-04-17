using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Interfaces;
using MediatR;

namespace InsuraTech.Application.Policies.Queries.GetMyClients;

public sealed class GetMyClientsHandler : IRequestHandler<GetMyClientsQuery, IEnumerable<ClientSummaryResponse>>
{
    private readonly IPolicyRepository _policyRepository;

    public GetMyClientsHandler(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<IEnumerable<ClientSummaryResponse>> Handle(
        GetMyClientsQuery request,
        CancellationToken cancellationToken)
    {
        var projections = await _policyRepository.GetMyClientsAsync(request.AdvisorId, cancellationToken);

        return projections.Select(p => new ClientSummaryResponse
        {
            DocumentId   = p.DocumentId,
            DocumentType = p.DocumentType,
            FirstName    = p.FirstName,
            LastName     = p.LastName,
            CityName     = p.CityName,
            PolicyCount  = p.PolicyCount
        });
    }
}